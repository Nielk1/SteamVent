//#define DEBUG_STEAMCMD_PARSE

//using Gameloop.Vdf;
//using Gameloop.Vdf.Linq;
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
using System.Runtime.CompilerServices;
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
        private const string SteamCmdDownloadURL = @"https://client-update.steamstatic.com/installer/steamcmd.zip";
        //private const string ANTI_STALL = $"-- type 'quit' to exit --";
        private const string ANTI_STALL = $"Loading Steam API...OK";

        //object procLock = new object();
        private readonly SemaphoreSlim ProcessLock = new SemaphoreSlim(1, 1);

        // ------------------------------------------------------------------
        // Readiness gate
        //
        // Downloading + extracting SteamCmd is an async prerequisite for every command. It is
        // modelled as a single shared Task rather than a plain semaphore, because that is what
        // gives us the guarantees a semaphore can't:
        //   * the download runs exactly once no matter how many callers race for it;
        //   * on failure every waiter observes the SAME descriptive exception instead of a
        //     downstream "steamcmd.exe missing" (a semaphore can't tell you *why* it opened);
        //   * it composes with await / Task.WhenAll / cancellation.
        // Every code path that launches steamcmd awaits EnsureReadyAsync() first.
        // ------------------------------------------------------------------
        private readonly object _readyGate = new object();
        private Task _readyTask;          // shared download/readiness task (null until first requested)

        // 1 = a fresh download just completed and the one-time activation ping has not yet run.
        private int _activationPending;

        // Application-lifetime cancellation. Shutdown() fires it, which aborts an in-flight
        // download and releases every caller blocked on the readiness gate (and the process
        // lock) so the UI can close even when a download is stuck.
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();

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

        /// <summary>
        /// Optional value for SteamCmd's <c>+force_install_dir</c>, i.e. the directory SteamCmd
        /// should store its data (steamapps, etc.) in. When set, <c>+force_install_dir "path"</c>
        /// is forced to the start of the parameter list on every SteamCmd launch. When null or
        /// empty the argument is not used at all.
        /// </summary>
        public string? ForceInstallDir { get; set; }

        /// <summary>
        /// Whether command entry points should block until SteamCmd is ready (true) or fail fast when
        /// it is not ready (false). This only affects the command methods (WorkshopStatusAsync,
        /// WorkshopDownloadItemAsync, ...): with it false they throw a
        /// <see cref="SteamCmdNotReadyException"/> instead of waiting. <see cref="EnsureReadyAsync"/>
        /// is unaffected and ALWAYS waits, so complex code can call it explicitly to opt into waiting
        /// -- and then catch/branch on the not-ready / failed outcome to react.
        /// </summary>
        public bool WaitWhenNotReady { get; set; } = false;

        /// <summary>
        /// Optional path to an EXISTING SteamCmd installation (the directory containing steamcmd.exe).
        /// When set, SteamCmd is NOT downloaded -- this path is used directly and merely validated.
        /// When null/empty SteamCmd is downloaded next to the application as usual.
        /// </summary>
        public string? SteamCmdPath { get; set; }

        /// <summary>The directory that holds the steamcmd.exe we launch (existing instance, or the default).</summary>
        public string SteamCmdDir
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SteamCmdPath))
                    return Path.GetFullPath(SteamCmdPath.Trim());

                return Path.Combine(AssemblyDirectory, "steamcmd");
            }
        }

        /// <summary>The full path to the steamcmd.exe we launch.</summary>
        public string SteamCmdExePath
        {
            get { return Path.Combine(SteamCmdDir, "steamcmd.exe"); }
        }

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
            Config.RegUnterminatedLinePatterns = (Config.UnterminatedLinePatterns ?? Array.Empty<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => new Regex(p))
                .ToArray();
            //Config.SteamCmdDownloadURL ?== DefaultSteamCmdDownloadURL; // enable this later

            // Allow steamvent.steamcmd.json to preset the forced install directory; callers
            // (e.g. BZRModManager) may still override it after the instance has been created.
            ForceInstallDir = string.IsNullOrWhiteSpace(Config.ForceInstallDir) ? null : Config.ForceInstallDir.Trim();
        }

        /// <summary>
        /// Builds the full SteamCmd command line, forcing <c>+force_install_dir</c> to the start
        /// of the parameter list when it is set. SteamCmd requires it to precede the other
        /// +commands for it to take effect. When it is not set the command is returned as-is.
        /// </summary>
        private string BuildSteamCmdCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(ForceInstallDir))
                return command;

            // Escape any embedded quotes so the path stays a single quoted argument.
            string dir = ForceInstallDir.Trim().Replace("\"", "\\\"");
            return $"+force_install_dir \"{dir}\" {command}";
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

        public async Task DownloadAsync(CancellationToken cancellationToken = default)
        {
            // Guarantee SteamCmd is downloaded & extracted before anything launches it. This is
            // the shared, deduplicated gate: it runs the download at most once and, on failure,
            // faults with a SteamCmdDownloadException that surfaces to the caller (instead of a
            // later, less useful "steamcmd.exe missing").
            await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);

            // One-time "activation" ping (primes the anonymous Steam session / creates the userdata
            // dir). Runs exactly once per fresh install; Interlocked.Exchange atomically claims it so
            // concurrent callers don't fire it twice.
            if (Interlocked.Exchange(ref _activationPending, 0) == 1)
            {
                await StartProcWithRetryAsync("+login anonymous +info +quit").ConfigureAwait(false);
            }
        }

        // ------------------------------------------------------------------
        // Readiness gate implementation (see the field comments above)
        // ------------------------------------------------------------------
        #region ReadinessGate

        /// <summary>True once SteamCmd has been downloaded and extracted and is ready to run.</summary>
        public bool IsReady
        {
            get { lock (_readyGate) return _readyTask?.IsCompletedSuccessfully == true; }
        }

        /// <summary>
        /// Ensures SteamCmd is ready to run -- validating the existing installation at
        /// <see cref="SteamCmdPath"/> when set, or downloading + extracting it when not -- and
        /// performs that setup exactly once no matter how many callers race for it. This method
        /// ALWAYS waits (it is the explicit opt-in to block for readiness); the command entry points
        /// only do so when <see cref="WaitWhenNotReady"/> is true. Awaits here block until the setup
        /// has either succeeded (this returns normally) or failed (a
        /// <see cref="SteamCmdDownloadException"/> is thrown).
        /// </summary>
        /// <param name="cancellationToken">
        /// Optional caller-scoped token that can release this wait early. The underlying setup is
        /// additionally bound to the application shutdown token, so shutdown always unblocks it.
        /// </param>
        public Task EnsureReadyAsync(CancellationToken cancellationToken = default)
        {
            Task ready;
            lock (_readyGate)
            {
                if (_readyTask == null)
                    _readyTask = SetupCoreAsync(_shutdownCts.Token);

                ready = _readyTask;
            }

            if (cancellationToken == default)
                return ready; // the shared task already observes the shutdown token

            return WaitForCancellation(ready, cancellationToken, _shutdownCts.Token);
        }

        /// <summary>
        /// Internal readiness gate for the command entry points. Honors <see cref="WaitWhenNotReady"/>:
        /// when true it waits for readiness exactly like <see cref="EnsureReadyAsync"/>; when false it
        /// fails fast -- throwing <see cref="SteamCmdNotReadyException"/> (or the real setup failure if
        /// one already occurred) instead of blocking. Complex code that wants to wait, or to react to
        /// the missing/failed state, should call <see cref="EnsureReadyAsync"/> explicitly instead.
        /// </summary>
        private async Task RequireReadyAsync(CancellationToken cancellationToken)
        {
            if (WaitWhenNotReady)
            {
                await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            // Fast-fail: never block. Inspect the shared setup task (if any) and surface a usable error.
            Task? shared;
            lock (_readyGate) shared = _readyTask;

            if (shared == null)
                throw new SteamCmdNotReadyException(
                    "SteamCmd is not ready and has not been set up. Call EnsureReadyAsync() to wait for it, " +
                    "point SteamCmdPath at an existing installation, or set WaitWhenNotReady = true.");

            if (shared.IsCompleted)
            {
                // Setup already finished: propagate its outcome (success, or the real setup/download failure).
                await shared.ConfigureAwait(false);
                return;
            }

            // Still in flight -- don't block for it.
            throw new SteamCmdNotReadyException(
                "SteamCmd is still being set up and WaitWhenNotReady is false. Call EnsureReadyAsync() to " +
                "wait for it, or set WaitWhenNotReady = true.");
        }

        /// <summary>
        /// Re-arms the readiness gate so the next <see cref="EnsureReadyAsync"/> performs a fresh
        /// download. Called by <see cref="Purge"/> so "Fix SteamCmd" rebuilds from scratch.
        /// </summary>
        public void ResetReadiness()
        {
            lock (_readyGate)
            {
                _readyTask = null;
            }
        }

        /// <summary>
        /// Signals application shutdown: aborts an in-flight download and releases every caller
        /// blocked on the readiness gate or the process lock. Idempotent; safe to call repeatedly.
        /// </summary>
        public void Shutdown()
        {
            try
            {
                _shutdownCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // already disposed -- nothing to do
            }
        }

        /// <summary>
        /// The application-scoped shutdown token that the readiness gate and download observe.
        /// </summary>
        public CancellationToken ShutdownToken => _shutdownCts.Token;

        #endregion ReadinessGate

        /// <summary>
        /// Makes SteamCmd ready exactly once (shared across callers): either validates the existing
        /// installation at <see cref="SteamCmdPath"/>, or downloads + extracts it (to a .part file,
        /// renamed into place only when complete, so an interrupted download never leaves a
        /// half-written zip that later looks valid). Any failure is reported as a
        /// <see cref="SteamCmdDownloadException"/> (or the cancellation exception when aborted by
        /// shutdown), which every waiter observes.
        /// </summary>
        private async Task SetupCoreAsync(CancellationToken cancellationToken)
        {
            string exePath = SteamCmdExePath;

            // Existing installation: we never download in this mode -- just validate it is present.
            if (!string.IsNullOrWhiteSpace(SteamCmdPath))
            {
                if (File.Exists(exePath))
                    return; // existing instance found -- ready
                throw new SteamCmdDownloadException(
                    $"SteamCmd is configured to use the existing installation at \"{SteamCmdDir}\", " +
                    "but steamcmd.exe was not found there.");
            }

            if (File.Exists(exePath))
                return; // already installed -- nothing to do

            await ProcessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Re-check under the lock: a sibling may have finished the download while we waited.
                if (File.Exists(exePath))
                    return;

                string zipPath = Path.Combine(AssemblyDirectory, "steamcmd.zip");
                string destDir = SteamCmdDir;

                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length == 0)
                {
                    string tempPath = zipPath + ".part";

                    OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Downloading));
                    using (HttpClient client = new HttpClient())
                    {
                        // Rely on our own cancellation instead of a fixed client timeout so a slow
                        // (but progressing) download isn't cut off, while a stuck one is aborted by
                        // the shutdown token.
                        client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
                        using (var response = await client
                                   .GetAsync(SteamCmdDownloadURL, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                   .ConfigureAwait(false))
                        {
                            if (!response.IsSuccessStatusCode)
                                throw new SteamCmdDownloadException(
                                    $"SteamCmd download failed: HTTP {(int)response.StatusCode} ({response.StatusCode}).");

                            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                await response.Content.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }

                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    File.Move(tempPath, zipPath);
                }

                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                // ZipFile.ExtractToDirectory cannot overwrite existing entries, so first clear any
                // stale (e.g. partially-extracted) files -- while preserving Steam's runtime data
                // (steamapps), which the archive does not contain and which we must never lose.
                foreach (string entry in Directory.EnumerateFileSystemEntries(destDir))
                {
                    if (Path.GetFileName(entry) == "steamapps")
                        continue;
                    try
                    {
                        if (Directory.Exists(entry))
                            RecursiveDelete(entry);
                        else
                            File.Delete(entry);
                    }
                    catch { /* best effort; a locked entry will surface as an extract failure below */ }
                }

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Extracting));
                ZipFile.ExtractToDirectory(zipPath, destDir);

                if (!File.Exists(exePath))
                    throw new SteamCmdDownloadException("SteamCmd extraction completed but steamcmd.exe was not found.");

                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Installed));

                // Claim the one-time activation ping (DownloadAsync fires it exactly once).
                Interlocked.Exchange(ref _activationPending, 1);
            }
            catch (OperationCanceledException)
            {
                // Shutdown / caller cancellation: report as cancellation, not a download failure.
                throw;
            }
            catch (Exception ex)
            {
                // Surface one descriptive failure to every waiter. Partial artifacts are left in
                // place; a retry re-downloads the zip and re-extracts with overwrite, so a failed
                // run is self-healing.
                throw new SteamCmdDownloadException("Failed to download or extract SteamCmd.", ex);
            }
            finally
            {
                ProcessLock.Release();
            }
        }

        /// <summary>
        /// Awaits <paramref name="task"/> while also honoring <paramref name="caller"/> and
        /// <paramref name="shutdown"/> so a cancelling caller is released promptly even while the
        /// shared download is still in flight.
        /// </summary>
        private static async Task WaitForCancellation(Task task, CancellationToken caller, CancellationToken shutdown)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(caller, shutdown);
            if (linked.IsCancellationRequested)
                throw new OperationCanceledException(linked.Token);

            Task finished = await Task.WhenAny(task, Task.Delay(System.Threading.Timeout.Infinite, linked.Token)).ConfigureAwait(false);
            if (!ReferenceEquals(finished, task))
                throw new OperationCanceledException(linked.Token);

            // The shared task finished: await it to propagate its result or exception.
            await task.ConfigureAwait(false);
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
            if (!Directory.Exists(SteamCmdDir)) throw new SteamCmdMissingException("steamcmd directory missing");
            if (!File.Exists(SteamCmdExePath)) throw new SteamCmdMissingException("steamcmd.exe missing");

            // Quote the exe path if it contains whitespace (external installations often do).
            string exeArg = SteamCmdExePath;
            if (exeArg.Any(System.Char.IsWhiteSpace))
                exeArg = $"\"{exeArg}\"";

            Process proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = AssemblyDirectory,
                    FileName = Path.Combine(AssemblyDirectory, "steamcmdprox.exe"),
                    Arguments = $"{exeArg} {command}",
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
            // Ensure SteamCmd is ready (waits, or fast-fails, per WaitWhenNotReady) before we launch it.
            await RequireReadyAsync(default).ConfigureAwait(false);

            // Apply +force_install_dir (if set) exactly once, here, so both the process
            // arguments and the logged argument line show the final command.
            string fullCommand = BuildSteamCmdCommand(command);

            await ProcessLock.WaitAsync(_shutdownCts.Token).ConfigureAwait(false);
            try
            {
                using Process proc = GetProc(fullCommand);

                OnSteamCmdArgs($"steamcmd.exe {fullCommand}");
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));

                proc.Start();
                // Commands are supplied entirely on the command line. Close the redirected
                // stdin writer immediately so SteamCMD sees EOF instead of an indefinitely
                // open input pipe.
                proc.StandardInput.Close();
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                StringBuilder AllOutput = new StringBuilder();

                await foreach (string line in ReadLines(proc).ConfigureAwait(false))
                    AllOutput.AppendLine(line);

                // ReadLines ends on stdout EOF. Confirm that the proxy process itself
                // has also exited before releasing ProcessLock to the next caller.
                await proc.WaitForExitAsync().ConfigureAwait(false);

                return AllOutput.ToString();
            }
            finally
            {
                OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));
                ProcessLock.Release();
            }
        }

        private async IAsyncEnumerable<string> ReadLines(Process proc)
        {
            StringBuilder lineBuffer = new StringBuilder();
            char[] readBuffer = new char[1];

            IEnumerable<string> FlushBuffer()
            {
                if (lineBuffer.Length == 0)
                    yield break;

                string text = lineBuffer.ToString();
                lineBuffer.Clear();

                OnSteamCmdOutputFull(text);

                foreach (string badstring in Config.BadStrings)
                    text = text.Replace(badstring + "\r\n", string.Empty);

                // SteamCmd sometimes fails to newline-terminate a logical line, concatenating the
                // next message onto its tail (e.g. "ERROR! ... (Access Denied).Unloading Steam API...OK").
                // Inject the missing CRLF so each logical line is emitted -- and later parsed -- as
                // its own line.
                text = InjectUnterminatedLineTerminators(text);

                if (text.Length == 0)
                    yield break;

                OnSteamCmdOutput(text);

                foreach (string outputLine in text.Split(
                    new[] { "\r\n" },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return outputLine;
                }
            }

            while (true)
            {
                int read;
                bool streamClosed = false;

                try
                {
                    // No inactivity timeout here. SteamCMD may legitimately remain silent.
                    // EOF is indicated only by ReadAsync returning 0.
                    read = await proc.StandardOutput.ReadAsync(
                        readBuffer,
                        0,
                        readBuffer.Length).ConfigureAwait(false);
                }
                catch (IOException)
                {
                    read = 0;
                    streamClosed = true;
                }
                catch (ObjectDisposedException)
                {
                    read = 0;
                    streamClosed = true;
                }
                catch (InvalidOperationException)
                {
                    read = 0;
                    streamClosed = true;
                }

                if (streamClosed || read == 0)
                {
                    foreach (string outputLine in FlushBuffer())
                        yield return outputLine;

                    yield break;
                }

                for (int i = 0; i < read; i++)
                {
                    lineBuffer.Append(readBuffer[i]);

                    int length = lineBuffer.Length;
                    if (length >= 2 &&
                        lineBuffer[length - 2] == '\r' &&
                        lineBuffer[length - 1] == '\n')
                    {
                        foreach (string outputLine in FlushBuffer())
                            yield return outputLine;
                    }
                }
            }
        }

        /// <summary>
        /// SteamCmd occasionally emits a logical line without a trailing newline, so the next
        /// message is concatenated onto its tail (e.g.
        /// "ERROR! Download item 123 failed (Access Denied).Unloading Steam API...OK"). Each entry
        /// in <see cref="ConfigData.UnterminatedLinePatterns"/> matches the terminal portion of one
        /// of those unterminated lines. Where such a pattern matches and is followed by further text
        /// on the same line -- i.e. it is not already at the end of the buffer and not already
        /// followed by a line break -- a CRLF is injected immediately after the match so the trailing
        /// text reads back in as its own line.
        /// </summary>
        private string InjectUnterminatedLineTerminators(string text)
        {
            if (string.IsNullOrEmpty(text) || Config?.RegUnterminatedLinePatterns == null)
                return text;

            foreach (Regex pattern in Config.RegUnterminatedLinePatterns)
            {
                if (pattern == null)
                    continue;

                text = pattern.Replace(text, m =>
                {
                    int end = m.Index + m.Length;

                    // A match that is at the very end of the buffer, or that is already followed by a
                    // line break, is already properly terminated -- leave it alone.
                    if (end >= text.Length || text[end] == '\r' || text[end] == '\n')
                        return m.Value;

                    return m.Value + "\r\n";
                });
            }

            return text;
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
            // Ensure SteamCmd is ready (waits, or fast-fails, per WaitWhenNotReady) before we launch it.
            await RequireReadyAsync(default).ConfigureAwait(false);

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

                string LibraryPath = ForceInstallDir ?? SteamCmdDir;

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

                    await ProcessLock.WaitAsync(_shutdownCts.Token);

                    // Apply +force_install_dir (if set) exactly once, here, so both the process
                    // arguments and the logged argument line show the final command.
                    string fullCommand = BuildSteamCmdCommand(command);

                    bool sawAntiStall = false;
                    for (int retries = 0; retries < 10 && !sawAntiStall; retries++)
                    {
                        OnStatus?.Invoke(ESteamCmdTaskStatus.Running);

                        using Process proc = GetProc(fullCommand);

                        OnSteamCmdArgs($"steamcmd.exe {fullCommand}");
                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));
                        proc.Start();
                        proc.StandardInput.Close();

                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                        int WorkshopReadStage = 0;
                        await foreach (string line in ReadLines(proc).ConfigureAwait(false))
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

                        // ReadLines ends on stdout EOF. Confirm the proxy process has
                        // also exited before allowing another SteamCMD request to start.
                        await proc.WaitForExitAsync().ConfigureAwait(false);

                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));

                        if (!sawAntiStall)
                            await Task.Delay(1000).ConfigureAwait(false);
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
                /*if (LatestUpdate.HasValue)
                {
                    await foreach (double? d in Web.SteamWorkshop.WorkshopStatusFromWebUpdateOnlyAsync(LibraryPath, AppId, LatestUpdate.Value, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressD = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC, ProgressD);
                    }
                }*/

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
            if (PublishedFileId < 100000)
                //throw new SteamCmdWorkshopDownloadException("Invalid ID");
                return null;
            
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

        /// <summary>
        /// How a failed workshop download should be treated by the caller, so retry policy can be
        /// applied per failure instead of blanket-retrying everything.
        /// </summary>
        public enum WorkshopItemFailureKind
        {
            /// <summary>Transient / unknown -- retrying is reasonable.</summary>
            Transient = 0,

            /// <summary>The item is unobtainable (e.g. "File Not Found", "Access Denied") -- retrying won't help.</summary>
            Permanent,

            /// <summary>Steam throttled or cut the connection (e.g. "No Connection") -- back off before retrying.</summary>
            Throttling,
        }

        /// <summary>
        /// The outcome of a single workshop item within a batched SteamCmd download run
        /// (see <see cref="WorkshopDownloadItemsAsync"/>).
        /// </summary>
        public sealed class WorkshopItemDownloadResult
        {
            /// <summary>The workshop item id this result refers to (0 when SteamCmd did not name one).</summary>
            public UInt64 PublishedFileId { get; }

            /// <summary>True when SteamCmd reported a "Success." line for this item.</summary>
            public bool Success { get; }

            /// <summary>SteamCmd's message describing the item's outcome.</summary>
            public string? Message { get; }

            /// <summary>
            /// For failures, how the caller should treat this result (see <see cref="WorkshopItemFailureKind"/>);
            /// always <see cref="WorkshopItemFailureKind.Transient"/> when <see cref="Success"/> is true.
            /// </summary>
            public WorkshopItemFailureKind FailureKind { get; }

            public WorkshopItemDownloadResult(UInt64 PublishedFileId, bool Success, string? Message,
                WorkshopItemFailureKind failureKind = WorkshopItemFailureKind.Transient)
            {
                this.PublishedFileId = PublishedFileId;
                this.Success = Success;
                this.Message = Message;
                this.FailureKind = failureKind;
            }

            public override string ToString()
            {
                return $"{(Success ? "Success" : "Failure")} - {PublishedFileId} - {Message}";
            }
        }

        // Extracts the item id out of SteamCmd's per-item workshop lines, e.g.
        //   "Success. Downloaded item 1325933293 to \"...\" (379069888 bytes)"
        //   "ERROR! Download item 1325933293 failed (File Not Found)."
        private static readonly Regex WorkshopItemIdRegex =
            new Regex(@"\bitem\s+(\d{6,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // A "plan" is the final, ready-to-run SteamCmd command plus an optional cleanup
        // (used to delete a generated batch script file once the run is finished).
        private readonly struct WorkshopBatchPlan
        {
            public readonly string Command;
            public readonly Action? Cleanup;

            public WorkshopBatchPlan(string command, Action? cleanup)
            {
                Command = command;
                Cleanup = cleanup;
            }
        }

        /// <summary>
        /// Download many workshop items in a single SteamCmd invocation (one process, not one
        /// per item). Per-item results are parsed from the streamed output and yielded in the
        /// order SteamCmd reports them, so callers can drive real progress. Small/medium
        /// batches go on the command line; large ones (near the OS command-line limit) are
        /// written to a script file run with <c>+runscript</c>, keeping <c>login</c>/<c>quit</c>
        /// on the command line so order is preserved (login, all items, quit).
        /// </summary>
        /// <param name="AppId">The app id (e.g. 443530 BZCC, 624970 BZ98R).</param>
        /// <param name="PublishedFileIds">Workshop item ids to download (duplicates/blanks dropped).</param>
        /// <param name="cancellationToken">Cancels the run and abandons the result stream.</param>
        /// <returns>One <see cref="WorkshopItemDownloadResult"/> per item, as reported.</returns>
        public async IAsyncEnumerable<WorkshopItemDownloadResult> WorkshopDownloadItemsAsync(
            UInt32 AppId,
            IEnumerable<UInt64> PublishedFileIds,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Ensure SteamCmd is ready (waits, or fast-fails, per WaitWhenNotReady) before we launch it.
            await RequireReadyAsync(cancellationToken).ConfigureAwait(false);

            // Normalise: drop invalid/blank ids and duplicates, preserving first-seen order.
            var ids = new List<UInt64>();
            var seen = new HashSet<UInt64>();
            if (PublishedFileIds != null)
            {
                foreach (var id in PublishedFileIds)
                {
                    if (id >= 100000 && seen.Add(id))
                        ids.Add(id);
                }
            }
            if (ids.Count == 0)
                yield break;

            WorkshopBatchPlan plan = BuildWorkshopBatchPlan(AppId, ids);
            string fullCommand = plan.Command;
            try
            {
                const int MaxAttempts = 3;
                for (int attempt = 1; attempt <= MaxAttempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    bool yieldedAny = false;
                    bool stalled = false;
                    string? lastLine = null;

                    await ProcessLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        using Process proc = GetProc(fullCommand);

                        OnSteamCmdArgs($"steamcmd.exe {fullCommand}");
                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Starting));

                        proc.Start();
                        // Commands are supplied entirely on the command line / in the script;
                        // close stdin immediately so SteamCmd sees EOF, not an open input pipe.
                        proc.StandardInput.Close();
                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Active));

                        try
                        {
                            await foreach (string line in ReadLines(proc).ConfigureAwait(false))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                if (!string.IsNullOrWhiteSpace(line))
                                    lastLine = line;

                                if (TryParseWorkshopDownloadResult(line, out WorkshopItemDownloadResult? r) && r != null)
                                {
                                    yieldedAny = true;
                                    OnSteamCmdRichOutput(new SteamCmdRichOutput(SteamCmdLogType.Workshop, line.TrimEnd('\r', '\n')));
                                    yield return r;
                                }
                            }

                            // ReadLines ends on stdout EOF; confirm the proxy process itself has
                            // exited before releasing the lock to the next caller.
                            await proc.WaitForExitAsync().ConfigureAwait(false);
                            stalled = IsStallLine(lastLine);
                        }
                        finally
                        {
                            // The consumer may have abandoned the stream before steamcmd
                            // finished; make sure we never leak a running steamcmd.exe.
                            try { if (!proc.HasExited) proc.Kill(); } catch { }
                        }
                    }
                    finally
                    {
                        OnSteamCmdStatusChange(new SteamCmdStatusChangeEventArgs(ESteamCmdStatus.Closed));
                        ProcessLock.Release();
                    }

                    // A run ending on "Checking for available updates..." with no items reported
                    // means SteamCmd bailed out before doing the work. Retry -- but only while we
                    // have not streamed any results, so nothing is ever yielded twice.
                    if (stalled && !yieldedAny && attempt < MaxAttempts)
                        continue;

                    yield break;
                }
            }
            finally
            {
                // Run the plan's cleanup (e.g. delete the batch script) exactly once, even if
                // the consumer abandons the stream early.
                plan.Cleanup?.Invoke();
            }
        }

        private WorkshopBatchPlan BuildWorkshopBatchPlan(UInt32 AppId, IReadOnlyList<UInt64> ids)
        {
            const int MaxCommandLineChars = 24000; // stay well under the ~32k Windows limit
            string[] items = ids.Select(id => $"workshop_download_item {AppId} {id}").ToArray();

            // 1) Everything on the command line (a single SteamCmd invocation).
            string cliBody = string.Join(" ", items.Select(i => "+" + i));
            string cliCommand = BuildSteamCmdCommand($"+login anonymous {cliBody} +quit");
            if (cliCommand.Length <= MaxCommandLineChars)
                return new WorkshopBatchPlan(cliCommand, null);

            // 2) Item commands in a script file, run with +runscript (no command-line limit).
            //    The surrounding +login / +quit stay on the command line, and the script is
            //    written into the process working directory and referenced by bare name so it
            //    resolves regardless of the directory SteamCmd expects scripts in.
            string scriptName = $"bzrmm_workshop_{Guid.NewGuid():N}.txt";
            string scriptPath = Path.Combine(AssemblyDirectory, "steamcmd", scriptName);
            File.WriteAllText(scriptPath, string.Join(Environment.NewLine, items));
            string scriptCommand = BuildSteamCmdCommand($"+login anonymous +runscript {scriptName} +quit");
            return new WorkshopBatchPlan(scriptCommand, () => { try { File.Delete(scriptPath); } catch { } });
        }

        private bool TryParseWorkshopDownloadResult(string line, out WorkshopItemDownloadResult? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            bool success;
            string? message;

            if (Config.RegWorkshopDownloadItemError.IsMatch(line))
            {
                success = false;
                message = Config.RegWorkshopDownloadItemError.Match(line).Groups["message"]?.Value;
            }
            else if (Config.RegWorkshopDownloadItemSuccess.IsMatch(line))
            {
                success = true;
                message = Config.RegWorkshopDownloadItemSuccess.Match(line).Groups["message"]?.Value;
            }
            else
            {
                return false;
            }

            var match = WorkshopItemIdRegex.Match(line);
            UInt64 id = 0;
            if (match.Success)
                UInt64.TryParse(match.Groups[1].Value, out id);

            var kind = success ? WorkshopItemFailureKind.Transient : ClassifyWorkshopFailure(message);
            result = new WorkshopItemDownloadResult(id, success, message, kind);
            return true;
        }

        /// <summary>
        /// Bucket a SteamCmd failure message so the caller can apply a sensible per-failure retry
        /// policy. Matching is intentionally narrow (only the known phrases) so anything unexpected
        /// falls through to <see cref="WorkshopItemFailureKind.Transient"/> and is still retried.
        /// </summary>
        private static WorkshopItemFailureKind ClassifyWorkshopFailure(string? message)
        {
            if (string.IsNullOrEmpty(message))
                return WorkshopItemFailureKind.Transient;

            // "This isn't going to work": the item doesn't exist / we don't have access to it.
            if (message.Contains("File Not Found", StringComparison.OrdinalIgnoreCase)
             || message.Contains("Access Denied", StringComparison.OrdinalIgnoreCase))
                return WorkshopItemFailureKind.Permanent;

            // Steam is throttling us or the connection dropped; hammering it more makes it worse.
            if (message.Contains("No Connection", StringComparison.OrdinalIgnoreCase))
                return WorkshopItemFailureKind.Throttling;

            return WorkshopItemFailureKind.Transient;
        }

        private static bool IsStallLine(string? line)
        {
            return string.Equals(
                line?.Trim(),
                @"[  0%] Checking for available updates...",
                StringComparison.Ordinal);
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

            // Re-arm the readiness gate: the binary was just removed, so the next
            // EnsureReadyAsync()/DownloadAsync() performs a fresh download.
            ResetReadiness();
        }
    }
}
