using Newtonsoft.Json;
using SteamVent;
using SteamVent.Common;
using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace SteamVentContextBridge
{
    internal class Program
    {
        static private ISteamClient? SteamClient { get; set; }
        static private Int32 Pipe { get; set; }
        static private Int32 User { get; set; }
        static private ISteamUGC? SteamUGC { get; set; }
        static private ISteamUtils? SteamUtils { get; set; }


        static async Task Main(string[] args)
        {
            UInt32 AppId = 0;
            if (args.Length == 0 || !UInt32.TryParse(args[0], out AppId) || AppId == 0)
            {
                Console.WriteLine("Usage: SteamVentContextBridge <AppId>");
                return;
            }

            string workingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
            workingDirectory = Path.Combine(workingDirectory, "steamventcontextbridge", AppId.ToString());
            if (!Directory.Exists(workingDirectory))
                Directory.CreateDirectory(workingDirectory);
            Directory.SetCurrentDirectory(workingDirectory);
            File.WriteAllText("steam_appid.txt", AppId.ToString());

            Steam.Load();
            SteamClient = Steam.CreateInterface<ISteamClient017>();
            Pipe = SteamClient.CreateSteamPipe();
            User = SteamClient.ConnectToGlobalUser(Pipe);
            SteamUtils = (ISteamUtils)SteamClient.GetISteamUtils<ISteamUtils007>(Pipe);
            SteamUGC = (ISteamUGC)SteamClient.GetISteamUGC<ISteamUGC005>(User, Pipe);

            SemaphoreSlim OutputLock = new SemaphoreSlim(1, 1);

            SemaphoreSlim OutputTasksLock = new SemaphoreSlim(1, 1);
            HashSet<Task> OutputTasks = new HashSet<Task>();

            for (; ; )
            {
                string? line = Console.ReadLine();
                if (line != null)
                {
                    if (line.Trim() == "exit")
                        break;

                    //string[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    string[] parts = line.Split(new char[] { ' ' }, 3);

                    if (parts.Length == 0 || parts[0].Length == 0)
                        continue;

                    if (parts[0] == "exit")
                        break;

                    if (parts[1] == "exit")
                        break;

                    switch(parts.Length > 1 ? parts[1] : null)
                    {
                        case "workshop":
                            {
                                parts = line.Split(new char[] { ' ' }, 4);
                                if (parts.Length < 3)
                                    break;
                                switch(parts[2])
                                {
                                    case "subscribe":
                                        {
                                            parts = line.Split(new char[] { ' ' }, 5);
                                            if (parts.Length < 4)
                                                break;
                                            ulong publishedFileId = 0;
                                            if (ulong.TryParse(parts[3], out publishedFileId))
                                            {
                                                await OutputTasksLock.WaitAsync();
                                                try
                                                {
                                                    OutputTasks.Add(Task.Run(async () =>
                                                    {
                                                        UInt64 steamworksCall = SteamUGC.SubscribeItem(publishedFileId);
                                                        IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RemoteStorageSubscribePublishedFileResult_t)));
                                                        try
                                                        {
                                                            bool failed = false;
                                                            while (!SteamUtils.IsAPICallCompleted(steamworksCall, ref failed) && !failed)
                                                            {
                                                                //System.Threading.Thread.Sleep(100);
                                                                await Task.Delay(100);
                                                            }
                                                            if (!failed && Steam.GetAPICallResult(Pipe, steamworksCall, pData, Marshal.SizeOf(typeof(RemoteStorageSubscribePublishedFileResult_t)), RemoteStorageSubscribePublishedFileResult_t.k_iCallback, ref failed))
                                                            {
                                                                RemoteStorageSubscribePublishedFileResult_t remoteStorageSubscribePublishedFileResult = (RemoteStorageSubscribePublishedFileResult_t)Marshal.PtrToStructure(pData, typeof(RemoteStorageSubscribePublishedFileResult_t));

                                                                await OutputLock.WaitAsync();
                                                                try
                                                                {
                                                                    //Console.WriteLine($"{parts[0]}\t{{\"nPublishedFileId\":{remoteStorageSubscribePublishedFileResult.m_nPublishedFileId}}}");
                                                                    Console.WriteLine($"{parts[0]}\t{JsonConvert.SerializeObject(remoteStorageSubscribePublishedFileResult)}");
                                                                }
                                                                finally
                                                                {
                                                                    OutputLock.Release();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                await OutputLock.WaitAsync();
                                                                try
                                                                {
                                                                    Console.WriteLine($"{parts[0]}\t{{\"error\":\"failed\"}}");
                                                                }
                                                                finally
                                                                {
                                                                    OutputLock.Release();
                                                                }
                                                            }
                                                        }
                                                        finally
                                                        {
                                                            Marshal.FreeHGlobal(pData);
                                                        }
                                                    }));
                                                }
                                                catch (Exception ex)
                                                {
                                                    await OutputLock.WaitAsync();
                                                    Console.WriteLine($"{parts[0]}\t{{\"error\":{JsonConvert.SerializeObject(ex)}}}");
                                                    OutputLock.Release();
                                                }
                                                finally
                                                {
                                                    OutputTasksLock.Release();
                                                }
                                            }
                                        }
                                        break;
                                    case "unsubscribe":
                                        {
                                            parts = line.Split(new char[] { ' ' }, 5);
                                            if (parts.Length < 4)
                                                break;
                                            ulong publishedFileId = 0;
                                            if (ulong.TryParse(parts[3], out publishedFileId))
                                            {
                                                await OutputTasksLock.WaitAsync();
                                                try
                                                {
                                                    OutputTasks.Add(Task.Run(async () =>
                                                    {
                                                        UInt64 steamworksCall = SteamUGC.UnsubscribeItem(publishedFileId);
                                                        IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RemoteStorageUnsubscribePublishedFileResult_t)));
                                                        try
                                                        {
                                                            bool failed = false;
                                                            while (!SteamUtils.IsAPICallCompleted(steamworksCall, ref failed) && !failed)
                                                            {
                                                                //System.Threading.Thread.Sleep(100);
                                                                await Task.Delay(100);
                                                            }
                                                            if (!failed && Steam.GetAPICallResult(Pipe, steamworksCall, pData, Marshal.SizeOf(typeof(RemoteStorageUnsubscribePublishedFileResult_t)), RemoteStorageUnsubscribePublishedFileResult_t.k_iCallback, ref failed))
                                                            {
                                                                RemoteStorageUnsubscribePublishedFileResult_t remoteStorageUnsubscribePublishedFileResult = (RemoteStorageUnsubscribePublishedFileResult_t)Marshal.PtrToStructure(pData, typeof(RemoteStorageUnsubscribePublishedFileResult_t));

                                                                await OutputLock.WaitAsync();
                                                                try
                                                                {
                                                                    //Console.WriteLine($"{parts[0]}\t{{\"nPublishedFileId\":{remoteStorageUnsubscribePublishedFileResult.m_nPublishedFileId}}}");
                                                                    Console.WriteLine($"{parts[0]}\t{JsonConvert.SerializeObject(remoteStorageUnsubscribePublishedFileResult)}");

                                                                }
                                                                finally
                                                                {
                                                                    OutputLock.Release();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                await OutputLock.WaitAsync();
                                                                try
                                                                {
                                                                    Console.WriteLine($"{parts[0]}\t{{\"error\":\"failed\"}}");
                                                                }
                                                                finally
                                                                {
                                                                    OutputLock.Release();
                                                                }
                                                            }
                                                        }
                                                        finally
                                                        {
                                                            Marshal.FreeHGlobal(pData);
                                                        }
                                                    }));
                                                }
                                                catch (Exception ex)
                                                {
                                                    await OutputLock.WaitAsync();
                                                    Console.WriteLine($"{parts[0]}\t{{\"error\":{JsonConvert.SerializeObject(ex)}}}");
                                                    OutputLock.Release();
                                                }
                                                finally
                                                {
                                                    OutputTasksLock.Release();
                                                }
                                            }
                                        }
                                        break;
                                    case "list":
                                        {
                                            await OutputTasksLock.WaitAsync();
                                            try
                                            {
                                                OutputTasks.Add(Task.Run(async () =>
                                                {
                                                    UInt32 count = SteamUGC.GetNumSubscribedItems();
                                                    UInt64[] pvecPublishedFileID = new UInt64[count];
                                                    count = SteamUGC.GetSubscribedItems(pvecPublishedFileID, count);
                                                    List<PublishedFileData> dataList = new List<PublishedFileData>();
                                                    for (int i = 0; i < count; i++)
                                                    {
                                                        UInt64 publishedFileId = pvecPublishedFileID[i];
                                                        EItemState state = SteamUGC.GetItemState(publishedFileId);
                                                        PublishedFileData data = new PublishedFileData(publishedFileId, state);
                                                        if (state.HasFlag(EItemState.k_EItemStateInstalled))
                                                        {
                                                            UInt64 punSizeOnDisk = 0;
                                                            StringBuilder pchFolder = new StringBuilder(1024);
                                                            UInt32 punTimeStamp = 0;
                                                            if (SteamUGC.GetItemInstallInfo(publishedFileId, ref punSizeOnDisk, pchFolder, (UInt32)pchFolder.Capacity, ref punTimeStamp))
                                                            {
                                                                data.punSizeOnDisk = punSizeOnDisk;
                                                                data.pchFolder = pchFolder.ToString();
                                                                data.punTimeStamp = punTimeStamp;
                                                            }
                                                        }
                                                        if (state.HasFlag(EItemState.k_EItemStateNeedsUpdate))
                                                        {
                                                            UInt64 punBytesDownloaded = 0;
                                                            UInt64 punBytesTotal = 0;
                                                            if (SteamUGC.GetItemDownloadInfo(publishedFileId, ref punBytesDownloaded, ref punBytesTotal))
                                                            {
                                                                data.punBytesDownloaded = punBytesDownloaded;
                                                                data.punBytesTotal = punBytesTotal;
                                                            }
                                                        }
                                                        dataList.Add(data);
                                                    }
                                                    await OutputLock.WaitAsync();
                                                    try
                                                    {
                                                        Console.WriteLine($"{parts[0]}\t{JsonConvert.SerializeObject(dataList)}");

                                                    }
                                                    finally
                                                    {
                                                        OutputLock.Release();
                                                    }
                                                }));
                                            }
                                            catch (Exception ex)
                                            {
                                                await OutputLock.WaitAsync();
                                                Console.WriteLine($"{parts[0]}\t{{\"error\":{JsonConvert.SerializeObject(ex)}}}");
                                                OutputLock.Release();
                                            }
                                            finally
                                            {
                                                OutputTasksLock.Release();
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            await OutputTasksLock.WaitAsync();
                                            try
                                            {
                                                OutputTasks.Add(Task.Run(async () =>
                                                {
                                                    await OutputLock.WaitAsync();
                                                    try
                                                    {
                                                        Console.WriteLine($"{parts[0]}\t{{\"error\":\"Unknown Command: {parts[1]} {parts[2]}\"}}");
                                                    }
                                                    finally
                                                    {
                                                        OutputLock.Release();
                                                    }
                                                }));
                                            }
                                            finally
                                            {
                                                OutputTasksLock.Release();
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        default:
                            {
                                await OutputTasksLock.WaitAsync();
                                try
                                {
                                    OutputTasks.Add(Task.Run(async () =>
                                    {
                                        await OutputLock.WaitAsync();
                                        Console.WriteLine($"{parts[0]}\t{{\"error\":\"Unknown Command: {parts[1]}\"}}");
                                        OutputLock.Release();
                                    }));
                                }
                                finally
                                {
                                    OutputTasksLock.Release();
                                }
                            }
                            break;
                    }
                }
            }

            // TODO find a way to remove tasks early
            await Task.WhenAll(OutputTasks);

            SteamClient.ReleaseUser(Pipe, User);
            SteamClient.BReleaseSteamPipe(Pipe);
        }
    }
}