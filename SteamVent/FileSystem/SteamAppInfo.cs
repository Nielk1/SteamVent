using System;
using System.Collections.Generic;
using System.IO;
using SteamVent.Core.BVdf;

namespace SteamVent.FileSystem
{
    /// <summary>Strongly-typed view of the <c>common</c> section of an app's appinfo.vdf record.</summary>
    public sealed class SteamAppInfoCommon
    {
        /// <summary>App type (e.g. "game", "demo", "dlc"), lower-cased. Null/empty when unset.</summary>
        public string? Type { get; init; }

        /// <summary>Display name.</summary>
        public string? Name { get; init; }

        /// <summary>Raw client-icon key (no directory / extension).</summary>
        public string? ClientIcon { get; init; }

        /// <summary>Developer names (the "associations" list, with the extended block as fallback).</summary>
        public IReadOnlyList<string> Developers { get; init; } = Array.Empty<string>();

        /// <summary>Publisher names (the "associations" list, with the extended block as fallback).</summary>
        public IReadOnlyList<string> Publishers { get; init; } = Array.Empty<string>();

        /// <summary>Original (non-Steam) release date, when present.</summary>
        public DateTime? OriginalReleaseDate { get; init; }

        /// <summary>Steam release date, when present.</summary>
        public DateTime? SteamReleaseDate { get; init; }

        /// <summary>Whether a library capsule asset is present.</summary>
        public bool HasLibraryCapsule { get; init; }
        /// <summary>Whether a library hero asset is present.</summary>
        public bool HasLibraryHero { get; init; }
        /// <summary>Whether a library logo asset is present.</summary>
        public bool HasLibraryLogo { get; init; }
    }

    /// <summary>Strongly-typed view of the <c>extended</c> section of an app's appinfo.vdf record.</summary>
    public sealed class SteamAppInfoExtended
    {
        public string? Developer { get; init; }
        public string? Publisher { get; init; }
    }

    /// <summary>
    /// A strongly-typed view of one app record from the Steam client appcache (appinfo.vdf), mapping the
    /// binary VDF chunk onto a clean POCO so callers never touch <see cref="BVPropertyCollection"/>.
    /// Produce instances through <see cref="SteamAppInfoDataFile.GetApps"/>.
    /// </summary>
    public sealed class SteamAppInfo
    {
        public UInt32 AppId { get; init; }
        public UInt32 State { get; init; }
        public UInt32 LastUpdate { get; init; }
        public UInt64 AccessToken { get; init; }
        public byte[] Checksum { get; init; } = Array.Empty<byte>();
        public UInt32 LastChangeNumber { get; init; }

        public SteamAppInfoCommon? Common { get; init; }
        public SteamAppInfoExtended? Extended { get; init; }

        /// <summary>App type (shortcut into <see cref="Common"/>), lower-cased. Null when unavailable.</summary>
        public string? Type => Common?.Type;

        /// <summary>Display name (shortcut into <see cref="Common"/>).</summary>
        public string? Name => Common?.Name;

        /// <summary>Absolute on-disk icon path, or null when the app has no client icon.</summary>
        public string? IconPath { get; init; }

        internal static SteamAppInfo? From(SteamAppInfoDataFile.SteamAppInfoDataFileChunk chunk)
        {
            BVPropertyCollection? data = chunk?.data;
            if (data == null)
                return null;

            BVPropertyCollection? appinfo = data["appinfo"] as BVPropertyCollection;
            BVPropertyCollection? common = appinfo?["common"] as BVPropertyCollection;
            BVPropertyCollection? extended = appinfo?["extended"] as BVPropertyCollection;
            if (common == null)
                return null;

            var developers = new List<string>();
            var publishers = new List<string>();
            if (common["associations"] is BVPropertyCollection associations)
            {
                foreach (BVProperty property in associations.Properties)
                {
                    if (property.Value is not BVPropertyCollection entry)
                        continue;
                    string? elemType = entry["type"]?.GetValue<string>();
                    string? elemVal = entry["name"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(elemVal))
                        continue;
                    if (elemType == "developer") developers.Add(elemVal);
                    else if (elemType == "publisher") publishers.Add(elemVal);
                }
            }

            string? extendedDeveloper = extended?["developer"]?.GetValue<string>();
            if (developers.Count == 0 && !string.IsNullOrWhiteSpace(extendedDeveloper))
                developers.Add(extendedDeveloper);
            string? extendedPublisher = extended?["publisher"]?.GetValue<string>();
            if (publishers.Count == 0 && !string.IsNullOrWhiteSpace(extendedPublisher))
                publishers.Add(extendedPublisher);

            long? originalReleaseDate = common["original_release_date"]?.GetValue<long>();
            long? steamReleaseDate = common["steam_release_date"]?.GetValue<long>();

            BVPropertyCollection? assets = common["library_assets"] as BVPropertyCollection;
            string? clientIcon = common["clienticon"]?.GetValue<string>();

            return new SteamAppInfo
            {
                AppId = chunk.AppID,
                State = chunk.State,
                LastUpdate = chunk.LastUpdate,
                AccessToken = chunk.AccessToken,
                Checksum = chunk.Checksum ?? Array.Empty<byte>(),
                LastChangeNumber = chunk.LastChangeNumber,
                Common = new SteamAppInfoCommon
                {
                    Type = common["type"]?.GetValue<string>()?.ToLowerInvariant(),
                    Name = common["name"]?.GetValue<string>(),
                    ClientIcon = clientIcon,
                    Developers = developers,
                    Publishers = publishers,
                    OriginalReleaseDate = originalReleaseDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(originalReleaseDate.Value).UtcDateTime : null,
                    SteamReleaseDate = steamReleaseDate.HasValue ? DateTimeOffset.FromUnixTimeSeconds(steamReleaseDate.Value).UtcDateTime : null,
                    HasLibraryCapsule = !string.IsNullOrWhiteSpace(assets?["library_capsule"]?.GetValue<string>()),
                    HasLibraryHero = !string.IsNullOrWhiteSpace(assets?["library_hero"]?.GetValue<string>()),
                    HasLibraryLogo = !string.IsNullOrWhiteSpace(assets?["library_logo"]?.GetValue<string>()),
                },
                Extended = new SteamAppInfoExtended
                {
                    Developer = extendedDeveloper,
                    Publisher = extendedPublisher,
                },
                IconPath = string.IsNullOrWhiteSpace(clientIcon)
                    ? null
                    : Path.Combine(SteamProcessInfo.SteamInstallPath, "steam", "games", clientIcon + ".ico"),
            };
        }
    }
}