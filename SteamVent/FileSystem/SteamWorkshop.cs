using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using SteamVent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.FileSystem
{
    public class SteamWorkshop
    {
        private static void UpdateProgress(IProgress<double?> progress, double p1, double p2, double p3)
        {
            if (progress == null)
                return;
            double percent =
                  (p1 * 0.45d) // folders
                + (p2 * 0.45d) // cache
                + (p3 * 0.1d); // html
            //Trace.WriteLine($"Progress: {percent}");
            progress.Report(percent);
        }

        public static async IAsyncEnumerable<double?> WorkshopStatusFromFilesAsync(string LibraryPath, UInt32 AppId, SemaphoreSlim DictionaryLock, Dictionary<UInt64, WorkshopItemStatus> WorkshopItems, Dictionary<UInt64, SemaphoreSlim> WorkshopItemLocks)
        {
            string ModsPath = Path.Combine(LibraryPath, "steamapps", "workshop", "content", AppId.ToString());
            if (Directory.Exists(ModsPath))
            {
                int ProgressCounter = 0;
                foreach (string path in Directory.EnumerateDirectories(ModsPath, "*", SearchOption.TopDirectoryOnly))
                {
                    string filename = Path.GetFileName(path);
                    UInt64 workshopId;
                    if (UInt64.TryParse(filename, out workshopId))
                    {
                        WorkshopItemStatus? currentItem = null;
                        SemaphoreSlim? itemLock = null;
                        try
                        {
                            await DictionaryLock.WaitAsync();
                            if (!WorkshopItems.ContainsKey(workshopId))
                            {
                                WorkshopItems[workshopId] = new WorkshopItemStatus
                                {
                                    WorkshopId = workshopId,
                                    Status = "installed",
                                    Size = -1,
                                    DateTime = null,
                                    HasUpdate = false,
                                    Missing = false,
                                    Detection = WorkshopItemStatus.WorkshopDetectionType.Folder,
                                };
                                WorkshopItemLocks[workshopId] = new SemaphoreSlim(1, 1);
                            }
                            else
                            {
                                itemLock = WorkshopItemLocks[workshopId];
                                currentItem = WorkshopItems[workshopId];
                            }
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
                                currentItem.Missing = false; // we have files so we can't be missing
                                currentItem.Detection |= WorkshopItemStatus.WorkshopDetectionType.Folder; // we have a folder so add detection
                            }
                            finally
                            {
                                itemLock.Release();
                            }
                        }

                        ProgressCounter++;
                        yield return (1d - (1d / (ProgressCounter + 1)));
                    }
                }
            }
            yield return 1d;
        }

        public static async IAsyncEnumerable<(double?, DateTime?)> WorkshopStatusFromCacheAsync(string LibraryPath, UInt32 AppId, SemaphoreSlim DictionaryLock, Dictionary<UInt64, WorkshopItemStatus> WorkshopItems, Dictionary<UInt64, SemaphoreSlim> WorkshopItemLocks)
        {
            string ManifestPath = Path.Combine(LibraryPath, "steamapps", "workshop", $"appworkshop_{AppId}.acf");
            if (File.Exists(ManifestPath))
            {
                int ProgressCounter = 0;

                HashSet<string> AcfKeys = new HashSet<string>();

                VProperty appWorkshop = VdfConvert.Deserialize(File.ReadAllText(ManifestPath));

                VObject WorkshopItemsInstalled = appWorkshop.Value["WorkshopItemsInstalled"] as VObject;
                foreach (VProperty prop in WorkshopItemsInstalled.Properties())
                    if (prop != null && prop.Key != "1")
                        AcfKeys.Add(prop.Key);

                VObject WorkshopItemDetails = appWorkshop.Value["WorkshopItemDetails"] as VObject;
                foreach (VProperty prop in WorkshopItemDetails.Properties())
                    if (prop != null && prop.Key != "1")
                        AcfKeys.Add(prop.Key);

                foreach (string workshopIdString in AcfKeys)
                {
                    UInt64 workshopId = 0;
                    if (!UInt64.TryParse(workshopIdString, out workshopId))
                        continue;

                    VObject? installRecord = WorkshopItemsInstalled[workshopIdString]?.Value<VObject>();
                    DateTime? timeupdatedInstalled = null;
                    long? size = null;
                    if (installRecord != null)
                    {
                        long? unix = installRecord["timeupdated"]?.Value<long>();
                        if (unix != null)
                            timeupdatedInstalled = DateTimeOffset.FromUnixTimeSeconds(unix.Value).DateTime; // UTC
                        size = installRecord["size"]?.Value<long>();
                    }

                    VObject? detailRecord = WorkshopItemDetails[workshopIdString]?.Value<VObject>();
                    DateTime? timeupdatedDetail = null;
                    if (detailRecord != null)
                    {
                        long? unix = detailRecord["timeupdated"]?.Value<long>();
                        if (unix != null)
                            timeupdatedDetail = DateTimeOffset.FromUnixTimeSeconds(unix.Value).DateTime; // UTC
                    }

                    bool timestampsDisagree = (timeupdatedInstalled != timeupdatedDetail);

                    WorkshopItemStatus currentItem = null;
                    SemaphoreSlim itemLock = null;
                    try
                    {
                        await DictionaryLock.WaitAsync();
                        if (!WorkshopItems.ContainsKey(workshopId))
                        {
                            WorkshopItems[workshopId] = new WorkshopItemStatus
                            {
                                WorkshopId = workshopId,
                                Status = timestampsDisagree ? "updated required" : "installed",
                                Size = size ?? -1,
                                DateTime = timeupdatedInstalled,
                                HasUpdate = timestampsDisagree,
                                Missing = true, // assume missing till we see the folder
                                Detection = WorkshopItemStatus.WorkshopDetectionType.Cache,
                            };
                            WorkshopItemLocks[workshopId] = new SemaphoreSlim(1, 1);
                        }
                        else
                        {
                            itemLock = WorkshopItemLocks[workshopId];
                            currentItem = WorkshopItems[workshopId];
                        }
                        //LatestUpdate = Nullable.Compare(LatestUpdate, timeupdatedInstalled) > 0 ? LatestUpdate : timeupdatedInstalled;
                        yield return (1d - (1d / (ProgressCounter + 1)), timeupdatedInstalled);
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
                            currentItem.Status = timestampsDisagree ? "updated required" : "installed";
                            if (currentItem.Size == -1 && size.HasValue)
                                currentItem.Size = size.Value;
                            currentItem.DateTime ??= timeupdatedInstalled;
                            currentItem.HasUpdate |= timestampsDisagree;
                            currentItem.Detection |= WorkshopItemStatus.WorkshopDetectionType.Cache; // we have a cache so add detection
                        }
                        finally
                        {
                            itemLock.Release();
                        }
                    }

                    ProgressCounter++;
                    yield return (1d - (1d / (ProgressCounter + 1)), null);
                }
            }

            yield return (1d, null);
        }

        public static async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(string LibraryPath, UInt32 AppId, IProgress<double?>? Progress = null)
        {
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

                // get existing mod folders
                Task DirectoryScanTask = Task.Run(async () =>
                {
                    await foreach (double? d in WorkshopStatusFromFilesAsync(LibraryPath, AppId, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressA = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                    }
                });

                // get existing cache files
                Task CacheScanTask = Task.Run(async () =>
                {
                    await foreach ((double?, DateTime?) d in WorkshopStatusFromCacheAsync(LibraryPath, AppId, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        LatestUpdate = Nullable.Compare(LatestUpdate, d.Item2) > 0 ? LatestUpdate : d.Item2;
                        ProgressB = d.Item1 ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                    }
                });

                await DirectoryScanTask;
                await CacheScanTask;

                // Read the workshop webpage because we can't get actual update information from steamcmd for anon accounts
                if (LatestUpdate.HasValue)
                {
                    await foreach (double? d in Web.SteamWorkshop.WorkshopStatusFromWebUpdateOnlyAsync(LibraryPath, AppId, LatestUpdate.Value, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressC = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                    }
                }

                try
                {
                    Progress.Report(1d);
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

    }
}
