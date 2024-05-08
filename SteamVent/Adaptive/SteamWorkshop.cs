using SteamVent.Bridge;
using SteamVent.Common;
using SteamVent.Common.InterProc;
using SteamVent.Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Adaptive
{
    public class SteamWorkshop
    {
        SteamContext context;
        Steamworks.SteamClient steamClient;
        public SteamWorkshop(SteamContext context, Steamworks.SteamClient steamClient)
        {
            this.context = context;
            this.steamClient = steamClient;
        }

        public async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(string LibraryPathOverride, UInt32 AppId, IProgress<double?>? Progress = null, bool AllowBridge = false)
        {
            string? LibraryPath = LibraryPathOverride ?? context.GetSteamApps().GetAppLibraryDir(AppId);
            if (LibraryPath == null)
                return null;
            return AllowBridge ? await FileSystem.SteamWorkshop.WorkshopStatusAsync(LibraryPath, AppId, Progress) : await WorkshopStatusAsync(LibraryPath, AppId, Progress);
        }

        private static void UpdateProgress(IProgress<double?> progress, double p1, double p2, double p3)
        {
            if (progress == null)
                return;
            double percent =
                  (p1 * 0.45d) // folders
                + (p2 * 0.45d) // cache
                + (p3 * 0.1d); // steam
            //Trace.WriteLine($"Progress: {percent}");
            progress.Report(percent);
        }

        private static async Task<List<WorkshopItemStatus>?> WorkshopStatusAsync(string LibraryPath, UInt32 AppId, IProgress<double?>? Progress = null)
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
                    await foreach (double? d in FileSystem.SteamWorkshop.WorkshopStatusFromFilesAsync(LibraryPath, AppId, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                    {
                        ProgressA = d ?? 1d;
                        if (Progress != null)
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
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
                            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                    }
                });

                await DirectoryScanTask;
                await CacheScanTask;

                // The steam cache should have the update need info so I guess we don't actually need this
                //if (LatestUpdate.HasValue)
                //{
                //    await foreach (double? d in Web.SteamWorkshop.WorkshopStatusFromWebUpdateOnlyAsync(LibraryPath, AppId, LatestUpdate.Value, DictionaryLock, WorkshopItems, WorkshopItemLocks))
                //    {
                //        ProgressC = d ?? 1d;
                //        if (Progress != null)
                //            UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                //    }
                //}

                // TODO consider moving this to another location that copies this function or something, or calls this one too
                // If moved this can also check for ownership before the call which would help
                //if (AllowBridge)
                {
                    ProgressC = 0;
                    int ProgressCounter = 0;
                    try
                    {
                        PublishedFileData[]? workshopList = null;
                        using (BridgeContext bc = new BridgeContext(AppId))
                        {
                            workshopList = await bc.SteamWorkshopListAsync();
                        }
                        if (workshopList != null)
                        {
                            foreach (PublishedFileData pub in workshopList)
                            {
                                UInt64 workshopId = pub.publishedFileId;
                                DateTime? DateTimeSet = pub.punTimeStamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(pub.punTimeStamp.Value).DateTime : null;

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
                                            Status = pub.state.HasFlag(EItemState.k_EItemStateNeedsUpdate) ? "update required" : "installed", // TODO revisit this
                                            //Size = (long?)pub.punBytesTotal ?? (long?)pub.punSizeOnDisk ?? -1,
                                            Size = (long?)pub.punSizeOnDisk ?? -1,
                                            DateTime = DateTimeSet,
                                            HasUpdate = pub.state.HasFlag(EItemState.k_EItemStateNeedsUpdate),
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
                                        currentItem.Status = pub.state.HasFlag(EItemState.k_EItemStateNeedsUpdate) ? "update required" : "installed"; // TODO revisit this
                                        if (pub.punSizeOnDisk.HasValue)
                                            currentItem.Size = (long)pub.punSizeOnDisk.Value;
                                        currentItem.DateTime = DateTimeSet.HasValue ? DateTimeSet : currentItem.DateTime;
                                        currentItem.HasUpdate |= pub.state.HasFlag(EItemState.k_EItemStateNeedsUpdate);
                                        currentItem.Detection |= WorkshopItemStatus.WorkshopDetectionType.Direct; // we have a direct so add detection
                                    }
                                    finally
                                    {
                                        itemLock.Release();
                                    }
                                }

                                ProgressCounter++;
                                ProgressC = 1d - (1d / (ProgressCounter + 1));
                                if (Progress != null)
                                    UpdateProgress(Progress, ProgressA, ProgressB, ProgressC);
                            }
                        }
                    }
                    catch
                    {
                        // process not found might happen
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
    }
}
