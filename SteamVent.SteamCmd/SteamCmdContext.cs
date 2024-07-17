//#define DEBUG_STEAMCMD_PARSE

//using Gameloop.Vdf;
//using Gameloop.Vdf.Linq;
using AngleSharp.Html.Parser;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Newtonsoft.Json;
using SteamVent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public enum SteamCmdLogType
    {
        Normal,
        Workshop,
    }
    public struct SteamCmdRichOutput
    {
        public SteamCmdLogType Type;
        public string Text;

        public SteamCmdRichOutput(SteamCmdLogType Type, string Text) : this()
        {
            this.Type = Type;
            this.Text = Text;
        }
    }

    public class SteamCmdContext
    {
        private const string SteamCmdDownloadURL = @"https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        //private const string ANTI_STALL = $"-- type 'quit' to exit --";
        private const string ANTI_STALL = $"Loading Steam API...OK";

        //object procLock = new object();
        SemaphoreSlim ProcessLock = new SemaphoreSlim(1, 1);
        public ESteamCmdStatus Status { get; private set; }

        public delegate void SteamCmdStatusChangeEventHandler(object sender, SteamCmdStatusChangeEventArgs e);
        public event SteamCmdStatusChangeEventHandler SteamCmdStatusChange;

        public delegate void SteamCmdOutputEventHandler(object sender, string msg);
        public event SteamCmdOutputEventHandler SteamCmdOutput;

        public delegate void SteamCmdOutputFullEventHandler(object sender, string msg);
        public event SteamCmdOutputFullEventHandler SteamCmdOutputFull;

        public delegate void SteamCmdRichOutputEventHandler(object sender, SteamCmdRichOutput msg);
        public event SteamCmdRichOutputEventHandler SteamCmdRichOutput;

        public delegate void SteamCmdArgsEventHandler(object sender, string msg);
        public event SteamCmdArgsEventHandler SteamCmdArgs;

        public ConfigData Config;

        private static readonly Lazy<SteamCmdContext> lazyInstance = new Lazy<SteamCmdContext>(() => new SteamCmdContext());
        public static SteamCmdContext Instance = lazyInstance.Value;
        private SteamCmdContext()
        {
            //Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText("steamvent.steamcmd.json"));
            Config = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(Path.Combine(AssemblyDirectory, "steamvent.steamcmd.json")));
            Config ??= new ConfigData();

            Config.RegWorkshopStatusItem = new Regex(Config.WorkshopStatusItem);
            Config.RegWorkshopDownloadItemError = new Regex(Config.WorkshopDownloadItemError);
            Config.RegWorkshopDownloadItemSuccess = new Regex(Config.WorkshopDownloadItemSuccess);
            //Config.SteamCmdDownloadURL ?== DefaultSteamCmdDownloadURL; // enable this later
        }
        public static string AssemblyDirectory
        {
            get
            {
                //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                //UriBuilder uri = new UriBuilder(codeBase);
                //string path = Uri.UnescapeDataString(uri.Path);
                //return Path.GetDirectoryName(path);
                string path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        public async Task TestRunAsync()
        {
            await StartProcWithRetryAsync($"+login anonymous +info +quit");
        }

        public async Task DownloadAsync()
        {
            try
            {
                await ProcessLock.WaitAsync();
                if (File.Exists(Path.Combine(AssemblyDirectory, "steamcmd\\steamcmd.exe"))) return;
                string steamcmdzip = Path.GetFileName(SteamCmdDownloadURL);
                if (!File.Exists(steamcmdzip))
                {
                    OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Downloading));
                    HttpClient client = new HttpClient();
                    var response = await client.GetAsync(SteamCmdDownloadURL);
                    using (var fs = new FileStream(Path.Combine(AssemblyDirectory, steamcmdzip), FileMode.CreateNew))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                if (!Directory.Exists(Path.Combine(AssemblyDirectory, "steamcmd"))) Directory.CreateDirectory(Path.Combine(AssemblyDirectory, "steamcmd"));
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Extracting));
                ZipFile.ExtractToDirectory(Path.Combine(AssemblyDirectory, steamcmdzip), Path.Combine(AssemblyDirectory, "steamcmd"));
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Installed));
            }
            finally
            {
                ProcessLock.Release();
            }

            await StartProcWithRetryAsync($"+login anonymous +info +quit");
        }

        private async Task<string> StartProcWithRetryAsync(string command)
        {
            string retVal = null;

            do
            {
                retVal = await StartProcAsync(command);

                OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Normal, retVal.TrimEnd('\r', '\n')));

            } while (retVal?.Trim().Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() == @"[  0%] Checking for available updates...");

            return retVal;
        }

        /// <summary>
        /// Get a wrapped steamcmd process but don't start it yet.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="SteamCmdMissingException"></exception>
        private Process GetProc(string command)
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

            return proc;
        }

        private async Task<string> StartProcAsync(string command)//, Action<string> LineOutput = null)
        {
            try
            {
                await ProcessLock.WaitAsync();
                Process proc = GetProc(command);

                OnSteamCmdArgs($"steamcmd.exe {command}");
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));
                proc.Start();
                //proc.BeginErrorReadLine();

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                StringBuilder AllOutput = new StringBuilder();

                await foreach(string line in ReadLines(proc))
                {
                    AllOutput.AppendLine(line);
                }

                // check if we're stuck on a prompt or something wierd
                if (!proc.HasExited)
                {
                    proc.Close();
                }

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));

                return AllOutput.ToString();
            }
            finally
            {
                ProcessLock.Release();
            }
        }

        private async IAsyncEnumerable<string> ReadLines(Process proc)
        {
            string line = string.Empty;
            char[] buffer = new char[1];
            do
            {
                Task<int> ReadCharTask = proc.StandardOutput.ReadAsync(buffer, 0, 1);
                if (await Task.WhenAny(ReadCharTask, Task.Delay(10000)) == ReadCharTask)
                {
                    if (!ReadCharTask.IsCompleted)
                    {
                        // we exited, which means the steamcmdprox detected a failure
                        if (proc.HasExited)
                        {
                            if (line.Length > 0)
                                foreach (string line2 in line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                    yield return line2;
                            yield break;
                        }
                    }
                    else
                    {
                        if (ReadCharTask.Result == 0)
                        {
                            if (line.Length > 0)
                            {
                                OnSteamCmdOutputFull(line);

                                foreach (string badstring in Config.BadStrings)
                                    while(line.Contains(badstring + "\r\n"))
                                        line = line.Replace(badstring + "\r\n", string.Empty);

                                OnSteamCmdOutput(line);

                                foreach (string line2 in line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                    yield return line2;
                            }
                            yield break;
                        }
                        line += buffer[0];
                        if (line.EndsWith("\r\n"))
                        {
                            OnSteamCmdOutputFull(line);

                            foreach (string badstring in Config.BadStrings)
                                while (line.Contains(badstring + "\r\n"))
                                    line = line.Replace(badstring + "\r\n", string.Empty);

                            OnSteamCmdOutput(line);

                            foreach (string line2 in line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                yield return line2;

                            line = string.Empty;
                        }
                        if (line == "Steam>")
                        {
                            OnSteamCmdOutputFull(line);

                            foreach (string badstring in Config.BadStrings)
                                while (line.Contains(badstring + "\r\n"))
                                    line = line.Replace(badstring + "\r\n", string.Empty);

                            OnSteamCmdOutput(line);

                            foreach (string line2 in line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                yield return line2;

                            yield break;
                        }
                        // TODO handle this better, but it means it stalled
                        if (line.Contains(@"Assertion Failed: CSteamEngine::BMainLoop appears to have stalled > 15 seconds without event signalled"))
                        {
                            // TODO forced exit of proxy?
                            yield break;
                        }
                    }
                }
                else
                {
                    OnSteamCmdOutputFull(line);

                    foreach (string badstring in Config.BadStrings)
                        while (line.Contains(badstring + "\r\n"))
                            line = line.Replace(badstring + "\r\n", string.Empty);

                    OnSteamCmdOutput(line);

                    foreach (string line2 in line.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        yield return line2;

                    yield break;
                }
            } while (!proc.StandardOutput.EndOfStream);
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
                                    foreach (string badstring in Config.BadStrings)
                                    {
                                        if (proc.StandardOutput.Peek() == badstring[0])
                                        {
#if DEBUG_STEAMCMD_PARSE
                                            Trace.WriteLine($"chew off badstring", "SteamCmdContext");
#endif
                                            for (int i = 0; i < (badstring.Length + 2); i++) // +2 for CRLF
                                            {
                                                proc.StandardOutput.Read();
                                            }
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
                                    foreach (string badstring in Config.BadStrings)
                                    {
                                        chars = chars.Replace(badstring + "\r\n", string.Empty);
                                    }
                                }
                                else
                                {
                                    bool foundBadString = false;
                                    foreach (string badstring in Config.BadStrings)
                                    {
                                        if (chars.EndsWith(badstring + "\r\n"))
                                        {
#if DEBUG_STEAMCMD_PARSE
                                            Trace.WriteLine($"removing badstring", "SteamCmdContext");
#endif
                                            // we have the bad string, so let's remove it as it's the real cause of our CRLF
                                            chars = chars.Replace(badstring + "\r\n", string.Empty);
                                            t = (char)0;
                                            SawCrCounter--; // our \r was caused by this badline, so lets drop back to 0
                                        }
                                        else
                                        {
#if DEBUG_STEAMCMD_PARSE
                                            Trace.WriteLine($"terminate read due to newline", "SteamCmdContext");
#endif
                                            // this is a normal end of line, we are good now
                                            foundBadString = true;
                                            break;
                                        }
                                    }
                                    if (foundBadString)
                                        break;
                                }
                            }
                        }
                        else
                        {
                            // we are null, which means it's time to timeout
                            if (timer >= timeout || proc.HasExited)
                            {
                                bool foundBadString = false;
                                foreach (string badstring in Config.BadStrings)
                                {
                                    if (chars.EndsWith(badstring))
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
                                        foundBadString = true;
                                        break;
                                    }
                                }
                                if (foundBadString)
                                    break;
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

        /*private void UpdateProgress(IProgress<double?> progress, params int[] args)
        {
            //lock (progress)
            {
                double sum = 0d;
                for (int i = 0; i < args.Length; i++)
                    sum += (1d - (1d / (args[i] + 1))) / args.Length;
                progress.Report(sum);
            }
        }*/
        private void UpdateProgress(IProgress<double?> progress, double p1, double p2, double p3, double p4)
        {
            if (progress == null)
                return;
            double percent =
                  (p1 * 0.1d) // folders
                + (p2 * 0.1d) // cache
                + (p3 * 0.7d) // steamcmd
                + (p4 * 0.1d); // html
            //Trace.WriteLine($"Progress: {percent}");
            progress.Report(percent);
        }

        public async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(UInt32 AppId, IProgress<double?>? Progress = null, Action<ESteamCmdTaskStatus>? OnStatus = null)
        {
            OnStatus?.Invoke(ESteamCmdTaskStatus.WaitingToStart);
            Trace.WriteLine($"WorkshopStatus({AppId})");
            try
            {
                Trace.Indent();

                Dictionary<UInt64, WorkshopItemStatus> WorkshopItems = new Dictionary<UInt64, WorkshopItemStatus>();
                Dictionary<UInt64, SemaphoreSlim> WorkshopItemLocks = new Dictionary<UInt64, SemaphoreSlim>();
                DateTime? LatestUpdate = null;
                SemaphoreSlim DictionaryLock = new SemaphoreSlim(1, 1);

                double ProgressA = 0;
                double ProgressB = 0;
                double ProgressC = 0;
                double ProgressD = 0;

                string LibraryPath = Path.Combine(SteamCmdContext.AssemblyDirectory, "steamcmd");

                // get existing mod folders
                Task DirectoryScanTask = Task.Run(async () =>
                {
                    await foreach (double? d in FileSystem.SteamWorkshop.WorkshopStatusFromFilesAsync(LibraryPath, AppId, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressA = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC, ProgressD);
                    }
                });

                // get existing cache files
                Task CacheScanTask = Task.Run(async () =>
                {
                    await foreach ((double?, DateTime?) d in FileSystem.SteamWorkshop.WorkshopStatusFromCacheAsync(LibraryPath, AppId, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        LatestUpdate = Nullable.Compare(LatestUpdate, d.Item2) > 0 ? LatestUpdate : d.Item2;
                        ProgressB = d.Item1 ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC, ProgressD);
                    }
                });

                try
                {
                    string command = $"+login anonymous +workshop_download_item {AppId} 1 +workshop_status {AppId} +quit";

                    // if it takes over a second to get the lock, status us as paused
                    Task lockTask = ProcessLock.WaitAsync();
                    CancellationTokenSource waitStatusCancel = new CancellationTokenSource();
                    await Task.WhenAny(lockTask, Task.Run(() => { Task.Delay(1000); OnStatus?.Invoke(ESteamCmdTaskStatus.Waiting); }, waitStatusCancel.Token));
                    await lockTask;
                    waitStatusCancel.Cancel();

                    bool sawAntiStall = false;
                    for (int retries = 0; retries < 10 && !sawAntiStall; retries++)
                    {
                        OnStatus?.Invoke(ESteamCmdTaskStatus.Running);

                        Process proc = GetProc(command);

                        OnSteamCmdArgs($"steamcmd.exe {command}");
                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));
                        proc.Start();
                        //proc.BeginErrorReadLine();

                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                        int WorkshopReadStage = 0;
                        await foreach (string line in ReadLines(proc))
                        {
                            if (line.Contains(ANTI_STALL))
                                sawAntiStall = true;

                            if (WorkshopReadStage == 2)
                            {
                                OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Normal, line.TrimEnd('\r', '\n')));
                                continue;
                            }

                            Match WorkshopStatusItemMatch = Config.RegWorkshopStatusItem.Match(line);
                            if (WorkshopReadStage == 0)
                                if (WorkshopStatusItemMatch.Success)
                                    WorkshopReadStage = 1;


                            if (WorkshopReadStage == 1)
                            {
                                if (WorkshopStatusItemMatch.Success)
                                {
                                    string datetimeString = $"{WorkshopStatusItemMatch.Groups["day"].Value} {WorkshopStatusItemMatch.Groups["month"].Value} {WorkshopStatusItemMatch.Groups["year"].Value} {WorkshopStatusItemMatch.Groups["hour"].Value}:{WorkshopStatusItemMatch.Groups["minutes"].Value}:{WorkshopStatusItemMatch.Groups["seconds"].Value}";
                                    DateTime parsedDateTime;

                                    string status = WorkshopStatusItemMatch.Groups["status"].Value;
                                    if (string.IsNullOrWhiteSpace(status))
                                        status = WorkshopStatusItemMatch.Groups["status2"].Value;

                                    string size = WorkshopStatusItemMatch.Groups["size"].Value;
                                    if (string.IsNullOrWhiteSpace(size))
                                        size = WorkshopStatusItemMatch.Groups["size3"].Value;

                                    bool foundDateTime = DateTime.TryParse(datetimeString, out parsedDateTime);
                                    if (WorkshopStatusItemMatch.Groups["workshopId"].Value == "1")
                                    {
                                        OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Normal, line.TrimEnd('\r', '\n')));
                                        continue;
                                    }

                                    OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Workshop, line.TrimEnd('\r', '\n')));

                                    //Trace.WriteLine($"Found workshop item {WorkshopStatusItemMatch.Groups["workshopId"].Value}");

                                    UInt64 workshopId = 0;
                                    if (!UInt64.TryParse(WorkshopStatusItemMatch.Groups["workshopId"].Value, out workshopId))
                                        continue;

                                    WorkshopItemStatus currentItem = null;
                                    SemaphoreSlim itemLock = null;
                                    try
                                    {
                                        await DictionaryLock.WaitAsync();
                                        DateTime? DateTimeSet = foundDateTime ? (DateTime?)TimeZone.CurrentTimeZone.ToUniversalTime(parsedDateTime) : null;
                                        if (!WorkshopItems.ContainsKey(workshopId))
                                        {
                                            WorkshopItems[workshopId] = new WorkshopItemStatus
                                            {
                                                WorkshopId = workshopId,
                                                Status = status,
                                                Size = long.Parse(size),
                                                DateTime = DateTimeSet,
                                                HasUpdate = WorkshopStatusItemMatch.Groups["status2"]?.Value == "updated required",
                                                Missing = true, // assume missing till we see the folder
                                                Detection = WorkshopItemStatus.WorkshopDetectionType.Direct,
                                            };
                                            WorkshopItemLocks[workshopId] = new SemaphoreSlim(1, 1);
                                        }
                                        else
                                        {
                                            itemLock = WorkshopItemLocks[workshopId];
                                            currentItem = WorkshopItems[workshopId];
                                        }
                                        LatestUpdate = Nullable.Compare(LatestUpdate, DateTimeSet) > 0 ? LatestUpdate : DateTimeSet;
                                    }
                                    finally
                                    {
                                        DictionaryLock.Release();
                                    }

                                    if (currentItem != null && itemLock != null)
                                    {
                                        try
                                        {
                                            await itemLock.WaitAsync();
                                            currentItem.Status = status;
                                            currentItem.Size = long.Parse(size);
                                            currentItem.DateTime ??= foundDateTime ? (DateTime?)TimeZone.CurrentTimeZone.ToUniversalTime(parsedDateTime) : null;
                                            currentItem.HasUpdate |= WorkshopStatusItemMatch.Groups["status2"]?.Value == "updated required";
                                            currentItem.Detection |= WorkshopItemStatus.WorkshopDetectionType.Direct; // we have a direct so add detection
                                        }
                                        finally
                                        {
                                            itemLock.Release();
                                        }
                                    }

                                    ProgressC++;
                                    if (Progress != null)
                                        UpdateProgress(Progress, ProgressA, ProgressB, ProgressC, ProgressD);
                                }
                                else
                                {
                                    WorkshopReadStage = 2;
                                    OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Normal, line.TrimEnd('\r', '\n')));
                                }
                            }
                            else
                            {
                                OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Normal, line.TrimEnd('\r', '\n')));
                            }
                        }

                        //await foreach (string line in ReadLines(proc)) { }

                        // check if we're stuck on a prompt or something wierd
                        if (!proc.HasExited)
                        {
                            proc.Close();
                        }

                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));

                        if (!sawAntiStall)
                            await Task.Delay(1000);
                    }
                }
                finally
                {
                    ProcessLock.Release();
                }
                OnStatus?.Invoke(ESteamCmdTaskStatus.Running);

                await DirectoryScanTask;
                await CacheScanTask;

                // Read the workshop webpage because we can't get actual update information from steamcmd for anon accounts
                if (LatestUpdate.HasValue)
                {
                    await foreach (double? d in Web.SteamWorkshop.WorkshopStatusFromWebUpdateOnlyAsync(LibraryPath, AppId, LatestUpdate.Value, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressD = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC, ProgressD);
                    }
                }

                try
                {
                    Progress?.Report(1d);
                    //await DictionaryLock.WaitAsync();
                    return WorkshopItems?.OrderBy(dr => dr.Key)?.Select(dr => dr.Value)?.ToList();
                }
                finally
                {
                    //DictionaryLock.Release();
                }
            }
            finally
            {
                Trace.Unindent();
            }
        }

        public async Task<string> WorkshopDownloadItemAsync(UInt32 AppId, UInt64 PublishedFileId)
        {
            string statusMessage = null;
            string statusType = null;
            string FullOutput = await StartProcWithRetryAsync($"+login anonymous +workshop_download_item {AppId} {PublishedFileId} +quit");
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

        protected void OnSteamCmdRichOutput(SteamCmdRichOutput msg)
        {
            SteamCmdRichOutput?.Invoke(this, msg);
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
            try
            {
                ProcessLock.Wait();

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
            finally
            {
                ProcessLock.Release();
            }
        }
    }
}
