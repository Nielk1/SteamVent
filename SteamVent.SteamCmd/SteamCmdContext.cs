//#define DEBUG_STEAMCMD_PARSE

//using Gameloop.Vdf;
//using Gameloop.Vdf.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class SteamCmdContext
    {
        private const string SteamCmdDownloadURL = @"https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        private const string badstring = "\\src\\common\\contentmanifest.cpp (650) : Assertion Failed: !m_bIsFinalized\r\n";
        private const string badstringShort = "\\src\\common\\contentmanifest.cpp (650) : Assertion Failed: !m_bIsFinalized";

        object procLock = new object();
        public ESteamCmdStatus Status { get; private set; }

        public delegate void SteamCmdStatusChangeEventHandler(object sender, SteamCmdStatusChangeEventArgs e);
        public event SteamCmdStatusChangeEventHandler SteamCmdStatusChange;

        public delegate void SteamCmdOutputEventHandler(object sender, string msg);
        public event SteamCmdOutputEventHandler SteamCmdOutput;

        public delegate void SteamCmdOutputFullEventHandler(object sender, string msg);
        public event SteamCmdOutputEventHandler SteamCmdOutputFull;

        public delegate void SteamCmdArgsEventHandler(object sender, string msg);
        public event SteamCmdArgsEventHandler SteamCmdArgs;

        public ConfigData Config;

        private static object InstanceLock = new object();
        private static SteamCmdContext Instance;
        private SteamCmdContext()
        {
            //Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText("steamvent.steamcmd.json"));
            Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(Path.Combine(AssemblyDirectory, "steamvent.steamcmd.json")));

            Config.RegWorkshopStatusItem = new Regex(Config.WorkshopStatusItem);
            Config.RegWorkshopDownloadItemError = new Regex(Config.WorkshopDownloadItemError);
            Config.RegWorkshopDownloadItemSuccess = new Regex(Config.WorkshopDownloadItemSuccess);
        }
        public static SteamCmdContext GetInstance()
        {
            lock(InstanceLock)
            {
                if (Instance == null)
                    Instance = new SteamCmdContext();
            }
            return Instance;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public void Download()
        {
            lock (procLock)
            {
                if (File.Exists(Path.Combine(AssemblyDirectory, "steamcmd\\steamcmd.exe"))) return;
                string steamcmdzip = Path.GetFileName(SteamCmdDownloadURL);
                if (!File.Exists(steamcmdzip))
                {
                    OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Downloading));
                    WebClient client = new WebClient();
                    client.DownloadFile(SteamCmdDownloadURL, Path.Combine(AssemblyDirectory, steamcmdzip));
                }
                if (!Directory.Exists(Path.Combine(AssemblyDirectory, "steamcmd"))) Directory.CreateDirectory(Path.Combine(AssemblyDirectory, "steamcmd"));
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Extracting));
                ZipFile.ExtractToDirectory(Path.Combine(AssemblyDirectory, steamcmdzip), Path.Combine(AssemblyDirectory, "steamcmd"));
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Installed));
            }

            StartProcWithRetry($"+login anonymous +status +quit");
        }

        private string StartProcWithRetry(string command)
        {
            string retVal = null;

            do
            {
                retVal = StartProc(command);
            } while (retVal?.Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() == @"[  0%] Checking for available updates...");

            return retVal;
        }

        private string StartProc(string command)//, Action<string> LineOutput = null)
        {
            lock (procLock)
            {
                if (!Directory.Exists(Path.Combine(AssemblyDirectory, "steamcmd"))) throw new SteamCmdMissingException("steamcmd directory missing");
                if (!File.Exists(Path.Combine(AssemblyDirectory, "steamcmd\\steamcmd.exe"))) throw new SteamCmdMissingException("steamcmd.exe missing");

                Process proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        WorkingDirectory = AssemblyDirectory,
                        FileName = Path.Combine(AssemblyDirectory, "steamcmdprox.exe"),
                        Arguments = $"steamcmd\\steamcmd.exe {command}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        StandardOutputEncoding = Encoding.Unicode,
                        //RedirectStandardError = true,
                        //StandardErrorEncoding = Encoding.Unicode,
                    }
                };

                OnSteamCmdArgs($"steamcmd.exe {command}");
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));
                proc.Start();
                //proc.BeginErrorReadLine();

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                StringBuilder AllOutput = new StringBuilder();
                while (!proc.StandardOutput.EndOfStream)// && !proc.HasExited) // note stream can still have data when proc is closed
                {
                    string line = ReadLine(proc);
                    //LineOutput?.Invoke(line);
                    AllOutput.AppendLine(line);
                    // do something with line
                }

                // check if we're stuck on a prompt or something wierd
                if(!proc.HasExited)
                {
                    proc.Close();
                }

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));

                return AllOutput.ToString();
            }
        }

        private string ReadLine(Process proc)
        {
            //lock (ioLock)
            {
                try
                {
                    string retVal = string.Empty;
                    string tmpVal = null;

                    do
                    {
                        tmpVal = ReadLineOrNullTimeout(proc, 100);
                        if (tmpVal == "\uffff") break;
                        retVal += tmpVal;
                    } while (tmpVal != null && !tmpVal.EndsWith("\r\n") && (retVal != "Steam>"));

                    return retVal;
                }
                finally
                {
                }
            }
        }

        /// <summary>
        /// Read a block of text from the SteamCmd console
        /// </summary>
        /// <param name="proc">SteamCmd Process</param>
        /// <param name="timeout">Time before an empty stream is considered empty</param>
        /// <returns>block of console text</returns>
        private string ReadLineOrNullTimeout(Process proc, int timeout)
        {
            Trace.WriteLine($"ReadLineOrNullTimeout({timeout})", "SteamCmdContext");

            //lock (ioLock)
            {
                //Trace.WriteLine($"ReadLineOrNullTimeout({timeout})[locked]", "SteamCmdContext");
                Trace.Indent();

                try
                {
                    int timer = 0;
                    string chars = string.Empty;
                    string charsAll = string.Empty;
                    char tP = '\0';
                    char t = '\0';
                    int SawCrCounter = 0;
                    bool forceOnce = true;

                    for (; ; )
                    {
                        if (forceOnce || proc.StandardOutput.Peek() > -1)
                        {
                            forceOnce = false;

                            tP = t;
                            int tn = proc.StandardOutput.Read();
                            t = (char)tn;
                            if (t == '\0')
                            {
#if DEBUG_STEAMCMD_PARSE
                                Trace.WriteLine($"terminate read due to nul", "SteamCmdContext");
#endif
                                break;
                            }
                            //if (tn > 255) break;
                            chars += t;
                            charsAll += t;
#if DEBUG_STEAMCMD_PARSE
                            Trace.WriteLine($"append '{chars.ToString().Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n")}' {tn}", "SteamCmdContext");
#endif
                            if (chars == "Steam>")
                            {
#if DEBUG_STEAMCMD_PARSE
                                Trace.WriteLine($"see prompt", "SteamCmdContext");
#endif
                                while (proc.StandardOutput.Peek() > -1)
                                {
                                    // this should only happen if we have more badstrings
                                    if (proc.StandardOutput.Peek() == badstring[0])
                                    {
#if DEBUG_STEAMCMD_PARSE
                                        Trace.WriteLine($"chew off badstring", "SteamCmdContext");
#endif
                                        for (int i = 0; i < badstring.Length; i++)
                                        {
                                            proc.StandardOutput.Read();
                                        }
                                    }
                                }
#if DEBUG_STEAMCMD_PARSE
                                Trace.WriteLine($"terminate read due to prompt", "SteamCmdContext");
#endif
                                break;
                            }
                            if (t == '\r')
                            {
                                SawCrCounter++;
                            }
                            if (tP == '\r' && t == '\n')
                            {
#if DEBUG_STEAMCMD_PARSE
                                Trace.WriteLine($"see newline", "SteamCmdContext");
#endif
                                // we have now have a CRLF
                                if (SawCrCounter > 1)
                                {
#if DEBUG_STEAMCMD_PARSE
                                    Trace.WriteLine($"badstring mid newline", "SteamCmdContext");
#endif
                                    // the only way this should happen is if we had a "bad string" get in the middle of a CRLF
                                    t = '\r'; // pretend we just read the pre-"bad string" character
                                    chars = chars.Replace(badstring, string.Empty);
                                }
                                else
                                {
                                    if (chars.EndsWith(badstring))
                                    {
#if DEBUG_STEAMCMD_PARSE
                                        Trace.WriteLine($"removing badstring", "SteamCmdContext");
#endif
                                        // we have the bad string, so let's remove it as it's the real cause of our CRLF
                                        chars = chars.Replace(badstring, string.Empty);
                                        t = (char)0;
                                        SawCrCounter--; // our \r was caused by this badline, so lets drop back to 0
                                    }
                                    else
                                    {
#if DEBUG_STEAMCMD_PARSE
                                        Trace.WriteLine($"terminate read due to newline", "SteamCmdContext");
#endif
                                        // this is a normal end of line, we are good now
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // we are null, which means it's time to timeout
                            if (timer >= timeout || proc.HasExited)
                            {
                                if (chars.EndsWith(badstringShort))
                                {
                                    // we have a wierd case here of a partial bad string
                                    forceOnce = true;
#if DEBUG_STEAMCMD_PARSE
                                    Trace.WriteLine($"timeout but in badstring missing newline, force parse continue", "SteamCmdContext");
#endif
                                }
                                else
                                {
#if DEBUG_STEAMCMD_PARSE
                                    Trace.WriteLine($"terminate read due to timeout", "SteamCmdContext");
#endif
                                    break;
                                }
                            }
                            else
                            {
                                Thread.Sleep(10);
                                timer += 10;
                            }
                        }
                    }

                    OnSteamCmdOutputFull(charsAll);
                    OnSteamCmdOutput(chars);
                    Trace.WriteLine($"return \"{chars.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n")}\"", "SteamCmdContext");
                    return chars;
                }
                finally
                {
                    Trace.Unindent();
                }
            }
        }

        public List<WorkshopItemStatus> WorkshopStatus(UInt32 AppId)
        {
            Trace.WriteLine($"WorkshopStatus({AppId})");
            try
            {
                Trace.Indent();

                // attempt to download any mods now in our manifest that we have folders for
                /*try
                {
                    HashSet<string> KnownDownloads = new HashSet<string>();
                    string ManifestPath = Path.Combine("steamcmd", "steamapps", "workshop", $"appworkshop_{AppId}.acf");
                    if (File.Exists(ManifestPath))
                    {
                        VProperty appWorkshop = VdfConvert.Deserialize(File.ReadAllText(ManifestPath));
                        appWorkshop.Value["WorkshopItemsInstalled"]?.Select(dr => (dr as VProperty)?.Key)?.Where(dr => dr != null)?.ToList()?.ForEach(dr => KnownDownloads.Add(dr));
                        appWorkshop.Value["WorkshopItemDetails"]?.Select(dr => (dr as VProperty)?.Key)?.Where(dr => dr != null)?.ToList()?.ForEach(dr => KnownDownloads.Add(dr));

                        string ModsPath = Path.Combine("steamcmd", "steamapps", "workshop", "content", AppId.ToString());
                        List<string> UnknownMods = Directory.EnumerateDirectories(ModsPath, "*", SearchOption.TopDirectoryOnly)
                            .Select(dr => Path.GetFileName(dr))
                            .Where(dr => !KnownDownloads.Contains(dr))
                            .ToList();

                        foreach(string mod in UnknownMods)
                        {
                            try
                            {
                                WorkshopDownloadItem(AppId, UInt64.Parse(mod));
                            }
                            catch { }
                        }
                    }
                }
                catch { }*/

                string RawString = StartProcWithRetry($"+login anonymous +workshop_download_item {AppId} 1 +workshop_status {AppId} +quit");

                /*try
                {
                    HashSet<string> KnownDownloads = new HashSet<string>();
                    string ManifestPath = Path.Combine("steamcmd", "steamapps", "workshop", $"appworkshop_{AppId}.acf");
                    if (File.Exists(ManifestPath))
                    {
                        VProperty appWorkshop = VdfConvert.Deserialize(File.ReadAllText(ManifestPath));
                        appWorkshop.Value["WorkshopItemsInstalled"]?.Select(dr => (dr as VProperty)?.Key)?.Where(dr => dr != null)?.ToList()?.ForEach(dr => KnownDownloads.Add(dr));
                        appWorkshop.Value["WorkshopItemDetails"]?.Select(dr => (dr as VProperty)?.Key)?.Where(dr => dr != null)?.ToList()?.ForEach(dr => KnownDownloads.Add(dr));

                        string ModsPath = Path.Combine("steamcmd", "steamapps", "workshop", "content", AppId.ToString());
                        List<string> UnknownMods = Directory.EnumerateDirectories(ModsPath, "*", SearchOption.TopDirectoryOnly)
                            .Select(dr => Path.GetFileName(dr))
                            .Where(dr => !KnownDownloads.Contains(dr))
                            .ToList();

                        if ((UnknownMods?.Count ?? 0) > 0)
                        {
                            string DisabledDir = Path.Combine("steamcmd", "steamapps", "workshop", "content", $"{AppId}_disabled");
                            if (!Directory.Exists(DisabledDir))
                                Directory.CreateDirectory(DisabledDir);
                            foreach (string mod in UnknownMods)
                            {
                                Directory.Move(Path.Combine("steamcmd", "steamapps", "workshop", "content", AppId.ToString(), mod), Path.Combine(DisabledDir, mod));
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                }*/

                List<WorkshopItemStatus> retVal =  RawString
                    .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(dr => Config.RegWorkshopStatusItem.Match(dr))
                    .Where(dr => dr.Success)
                    .Select(dr =>
                    {
                        string datetimeString = $"{dr.Groups["day"].Value} {dr.Groups["month"].Value} {dr.Groups["year"].Value} {dr.Groups["hour"].Value}:{dr.Groups["minutes"].Value}:{dr.Groups["seconds"].Value}";
                        DateTime parsedDateTime;

                        string status = dr.Groups["status"].Value;
                        if (string.IsNullOrWhiteSpace(status))
                            status = dr.Groups["status2"].Value;

                        string size = dr.Groups["size"].Value;
                        if (string.IsNullOrWhiteSpace(size))
                            size = dr.Groups["size3"].Value;

                        bool foundDateTime = DateTime.TryParse(datetimeString, out parsedDateTime);
                        if (dr.Groups["workshopId"].Value == "1") return null;

                        Trace.WriteLine($"Found workshop item {dr.Groups["workshopId"].Value}");

                        return new WorkshopItemStatus()
                        {
                            WorkshopId = UInt64.Parse(dr.Groups["workshopId"].Value),
                            Status = status,
                            Size = long.Parse(size),
                            DateTime = foundDateTime ? (DateTime?)parsedDateTime : null,
                            HasUpdate = dr.Groups["status2"]?.Value == "updated required",
                        };
                    })
                    .Where(dr => dr != null)
                    .ToList();

                {
                    HashSet<UInt64> KnownIDs = new HashSet<UInt64>();
                    foreach (var item in retVal)
                    {
                        KnownIDs.Add(item.WorkshopId);
                    }

                    string ModsPath = Path.Combine(SteamCmdContext.AssemblyDirectory, "steamcmd", "steamapps", "workshop", "content", AppId.ToString());
                    List<string> UnknownMods = Directory.EnumerateDirectories(ModsPath, "*", SearchOption.TopDirectoryOnly)
                        .Select(dr => Path.GetFileName(dr))
                        .Where(dr =>
                        {
                            try
                            {
                                return !KnownIDs.Contains(UInt64.Parse(dr));
                            }
                            catch { }
                            return false;
                        }).ToList();
                    foreach (string UnknownMod in UnknownMods)
                    {
                        retVal.Add(new WorkshopItemStatus()
                        {
                            WorkshopId = UInt64.Parse(UnknownMod),
                            FolderOnlyDetection = true,
                        });
                    }
                }

                return retVal;
            }
            finally
            {
                Trace.Unindent();
            }
        }

        public string WorkshopDownloadItem(UInt32 AppId, UInt64 PublishedFileId)
        {
            string statusMessage = null;
            string statusType = null;
            string FullOutput = StartProcWithRetry($"+login anonymous +workshop_download_item {AppId} {PublishedFileId} +quit");
            string[] OutputLines = FullOutput.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string OutputLine in OutputLines)
            {
                bool found = false;
                if(Config.RegWorkshopDownloadItemError.IsMatch(OutputLine))
                {
                    statusMessage = Config.RegWorkshopDownloadItemError.Match(OutputLine).Groups["message"]?.Value;
                    statusType = @"ERROR!";
                    found = true;
                    break;
                }
                if (found) break;

                if (Config.RegWorkshopDownloadItemSuccess.IsMatch(OutputLine))
                {
                    statusMessage = Config.RegWorkshopDownloadItemSuccess.Match(OutputLine).Groups["message"]?.Value;
                    statusType = @"Success";
                    found = true;
                    break;
                }
                if (found) break;
            }
            if (statusType == @"ERROR!")
            {
                string errorText = statusMessage;
                throw new SteamCmdWorkshopDownloadException(errorText);
            }
            else if (statusType == "Success")
            {
                string successText = statusMessage;
                //string tmp = @"Downloaded item 1325933293 to ""D:\Data\Programming\BZRModManager\BZRModManager\bin\steamcmd\steamapps\workshop\content\624970\1325933293"" (379069888 bytes)";
                return successText;
            }
            else
            {
                Exception ex = new SteamCmdException("Unknown Error", new SteamCmdWorkshopDownloadException(FullOutput));
                throw ex;
            }
        }

        protected void OnSteamCmdStatusChange(SteamCmdStatusChangeEventArgs e)
        {
            Status = e.Status;
            SteamCmdStatusChange?.Invoke(this, e);
        }

        protected void OnSteamCmdOutput(string msg)
        {
            SteamCmdOutput?.Invoke(this, msg);
        }

        protected void OnSteamCmdOutputFull(string msg)
        {
            SteamCmdOutputFull?.Invoke(this, msg);
        }

        protected void OnSteamCmdArgs(string msg)
        {
            SteamCmdArgs?.Invoke(this, msg);
        }


        private void RecursiveDelete(string path)
        {
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                File.Delete(file);

            foreach (string dir in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                RecursiveDelete(dir);
            }
            Directory.Delete(path);
        }

        public void Purge()
        {
            lock (procLock)
            {
                if (Directory.Exists("steamcmd"))
                {
                    foreach (string file in Directory.EnumerateFiles("steamcmd", "*", SearchOption.TopDirectoryOnly))
                        File.Delete(file);

                    foreach (string dir in Directory.EnumerateDirectories("steamcmd", "*", SearchOption.TopDirectoryOnly))
                    {
                        if (Path.GetFileName(dir) == "steamapps")
                            continue;

                        RecursiveDelete(dir);
                    }

                    {
                        foreach (string file in Directory.EnumerateFiles(Path.Combine("steamcmd", "steamapps"), "*", SearchOption.TopDirectoryOnly))
                            File.Delete(file);

                        foreach (string dir in Directory.EnumerateDirectories(Path.Combine("steamcmd", "steamapps"), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (Path.GetFileName(dir) == "workshop")
                                continue;

                            RecursiveDelete(dir);
                        }
                    }

                    {
                        foreach (string file in Directory.EnumerateFiles(Path.Combine("steamcmd", "steamapps", "workshop"), "*", SearchOption.TopDirectoryOnly))
                            File.Delete(file);

                        foreach (string dir in Directory.EnumerateDirectories(Path.Combine("steamcmd", "steamapps", "workshop"), "*", SearchOption.TopDirectoryOnly))
                        {
                            if (Path.GetFileName(dir) == "content")
                                continue;

                            RecursiveDelete(dir);
                        }
                    }
                }
            }
        }
    }
}
