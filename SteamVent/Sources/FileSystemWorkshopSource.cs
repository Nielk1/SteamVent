using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SteamVent.Core;
using SteamVent.Core.InterProc;
using SteamVent.FileSystem;

namespace SteamVent
{
    /// <summary>
    /// Local workshop state: the items physically present under
    /// <c>&lt;library&gt;/steamapps/workshop/content/{appid}</c>. This is a filesystem-only fallback (lowest
    /// priority) - the live Bridge / SteamCmd sources, when present, provide richer and more current data.
    /// </summary>
    public sealed class FileSystemWorkshopSource : ISource, IWorkshopSource
    {
        public string Name => "FileSystem.Workshop";
        public int Priority => 30;
        public SteamSource Family => SteamSource.FileSystem;

        public bool IsAvailable() => !string.IsNullOrEmpty(SteamProcessInfo.SteamInstallPath);
        public Task<bool> CheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsAvailable());

        public Task<IReadOnlyCollection<PublishedFileData>> GetInstalledWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => Task.FromResult(Build(appid));

        public Task<IReadOnlyCollection<PublishedFileData>> GetSubscribedWorkshopItems(int appid, CancellationToken cancellationToken = default)
            => GetInstalledWorkshopItems(appid, cancellationToken);

        private IReadOnlyCollection<PublishedFileData> Build(int appid)
        {
            var result = new List<PublishedFileData>();
            var seen = new HashSet<ulong>();
            foreach (var library in SteamProcessInfo.GetSteamLibraryPaths())
            {
                if (string.IsNullOrEmpty(library)) continue;
                string contentDir = Path.Combine(library, "steamapps", "workshop", "content", appid.ToString());
                if (!Directory.Exists(contentDir)) continue;
                foreach (var dir in Directory.EnumerateDirectories(contentDir))
                {
                    if (!ulong.TryParse(Path.GetFileName(dir), out var id)) continue;
                    if (!seen.Add(id)) continue;
                    result.Add(new PublishedFileData(id, EItemState.k_EItemStateInstalled) { Folder = dir });
                }
            }
            return result;
        }
    }
}