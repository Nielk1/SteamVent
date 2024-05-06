using Newtonsoft.Json;
using SteamVent.Common.InterProc;
using SteamVent.InterProc.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Bridge
{
    public class BridgeContext : IDisposable
    {
        private static SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim WriteLock = new SemaphoreSlim(1, 1);
        private Process proc;
        private UInt32 appId;
        private bool exited;
        public BridgeContext(UInt32 appId)
        {
            this.appId = appId;
            this.exited = false;
            this.proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "SteamVentContextBridge.exe"),
                    Arguments = $"{appId}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    //StandardOutputEncoding = Encoding.Unicode,
                    //RedirectStandardError = true,
                    //StandardErrorEncoding = Encoding.Unicode,
                }
            };

            proc.Start();

            Task.Run(async () =>
            {
                for (; ; )
                {
                    string? line = await proc.StandardOutput.ReadLineAsync();

                    if (line == null)
                        break;

                    string[] parts = line.Split(new char[] { '\t' }, 2);

                    await ResponseWaitLocksLock.WaitAsync();
                    try
                    {
                        if (ResponseWaitLocks.ContainsKey(parts[0]))
                        {
                            (SemaphoreSlim, string) item = ResponseWaitLocks[parts[0]];
                            item.Item2 = parts[1];
                            ResponseWaitLocks[parts[0]] = item;
                            item.Item1.Release();
                        }
                    }
                    finally
                    {
                        ResponseWaitLocksLock.Release();
                    }
                }
                exited = true;
            });
        }


        #region Dispose
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Shutdown();
            }

            while (!exited)
                Thread.Sleep(1000);

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~BridgeContext()
        {
            Dispose(false);
        }
        #endregion Dispose

        private void Shutdown()
        {
            WriteLock.Wait();
            try
            {
                proc.StandardInput.WriteLine($"exit");
            }
            finally
            {
                WriteLock.Release();
            }
        }

        public delegate void BridgeOutputEventHandler(object sender, string msg);
        public event BridgeOutputEventHandler BridgeOutput;
        protected void OnBridgeOutput(string msg)
        {
            BridgeOutput?.Invoke(this, msg);
        }


        SemaphoreSlim ResponseWaitLocksLock = new SemaphoreSlim(1, 1);
        Dictionary<string, (SemaphoreSlim, string)> ResponseWaitLocks = new Dictionary<string, (SemaphoreSlim, string)>();
        int RequestCounter = 0;

        public async Task<RemoteStorageSubscribePublishedFileResult_t?> SteamWorkshopSubscribeAsync(UInt64 publishedFileId)
        {
            int localCounter = 0;
            SemaphoreSlim mySemaphore = new SemaphoreSlim(0, 1);
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                localCounter = RequestCounter;
                RequestCounter++;
                ResponseWaitLocks[localCounter.ToString()] = (mySemaphore, null);
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
            await WriteLock.WaitAsync();
            try
            {
                proc.StandardInput.WriteLine($"{localCounter} workshop subscribe {publishedFileId}");
            }
            finally
            {
                WriteLock.Release();
            }
            await Task.WhenAny(mySemaphore.WaitAsync(), Task.Delay(1000 * 10));
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                string? response = ResponseWaitLocks[localCounter.ToString()].Item2;
                RemoteStorageSubscribePublishedFileResult_t? retVal = null;
                bool success = false;
                // TODO deal with error JSON instead of just null from timeout
                if (response == null)
                {
                    success = false;
                }
                else
                {
                    retVal = JsonConvert.DeserializeObject<RemoteStorageSubscribePublishedFileResult_t>(response);
                }
                ResponseWaitLocks.Remove(localCounter.ToString());
                return retVal;
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
        }

        public async Task<RemoteStorageUnsubscribePublishedFileResult_t?> SteamWorkshopUnsubscribeAsync(UInt64 publishedFileId)
        {
            int localCounter = 0;
            SemaphoreSlim mySemaphore = new SemaphoreSlim(0, 1);
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                localCounter = RequestCounter;
                RequestCounter++;
                ResponseWaitLocks[localCounter.ToString()] = (mySemaphore, null);
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
            await WriteLock.WaitAsync();
            try
            {
                proc.StandardInput.WriteLine($"{localCounter} workshop unsubscribe {publishedFileId}");
            }
            finally
            {
                WriteLock.Release();
            }
            await Task.WhenAny(mySemaphore.WaitAsync(), Task.Delay(1000 * 10));
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                string? response = ResponseWaitLocks[localCounter.ToString()].Item2;
                RemoteStorageUnsubscribePublishedFileResult_t? retVal = null;
                bool success = false;
                // TODO deal with error JSON instead of just null from timeout
                if (response == null)
                {
                    success = false;
                }
                else
                {
                    retVal = JsonConvert.DeserializeObject<RemoteStorageUnsubscribePublishedFileResult_t>(response);
                }
                ResponseWaitLocks.Remove(localCounter.ToString());
                return retVal;
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
        }

        public async Task<PublishedFileData[]?> SteamWorkshopListAsync()
        {
            int localCounter = 0;
            SemaphoreSlim mySemaphore = new SemaphoreSlim(0, 1);
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                localCounter = RequestCounter;
                RequestCounter++;
                ResponseWaitLocks[localCounter.ToString()] = (mySemaphore, null);
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
            await WriteLock.WaitAsync();
            try
            {
                proc.StandardInput.WriteLine($"{localCounter} workshop list");
            }
            finally
            {
                WriteLock.Release();
            }
            await Task.WhenAny(mySemaphore.WaitAsync(), Task.Delay(1000 * 10));
            await ResponseWaitLocksLock.WaitAsync();
            try
            {
                string? response = ResponseWaitLocks[localCounter.ToString()].Item2;
                PublishedFileData[]? retVal = null;
                bool success = false;
                // TODO deal with error JSON instead of just null from timeout
                if (response == null)
                {
                    success = false;
                }
                else
                {
                    retVal = JsonConvert.DeserializeObject<PublishedFileData[]>(response);
                }
                ResponseWaitLocks.Remove(localCounter.ToString());
                return retVal;
            }
            finally
            {
                ResponseWaitLocksLock.Release();
            }
        }
    }
}
