using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using SteamVent.Core.BVdf;

namespace SteamVent.FileSystem
{
    /// <summary>
    /// Strongly-typed view of the <c>common</c> section of an app's appinfo.vdf record, with every non-elevated
    /// key preserved in <see cref="BvdfNode.Extra"/>.
    /// </summary>
    public sealed class SteamAppInfoCommon : BvdfNode
    {
        /// <summary>App type (e.g. "game", "demo", "dlc"), lower-cased. Null/empty when unset.</summary>
        [DataMember(Name = "type")]
        public string? Type { get; init; }

        /// <summary>Display name.</summary>
        [DataMember(Name = "name")]
        public string? Name { get; init; }

        /// <summary>Raw client-icon key (no directory / extension).</summary>
        [DataMember(Name = "clienticon")]
        public string? ClientIcon { get; init; }

        /// <summary>Developer names (the "associations" list, with the extended block as fallback).</summary>
        public IReadOnlyList<string> Developers { get; init; } = Array.Empty<string>();

        /// <summary>Publisher names (the "associations" list, with the extended block as fallback).</summary>
        public IReadOnlyList<string> Publishers { get; init; } = Array.Empty<string>();

        /// <summary>Original (non-Steam) release date, when present.</summary>
        [DataMember(Name = "original_release_date")]
        public DateTime? OriginalReleaseDate { get; init; }

        /// <summary>Steam release date, when present.</summary>
        [DataMember(Name = "steam_release_date")]
        public DateTime? SteamReleaseDate { get; init; }

        /// <summary>Whether a library capsule asset is present.</summary>
        public bool HasLibraryCapsule { get; init; }
        /// <summary>Whether a library hero asset is present.</summary>
        public bool HasLibraryHero { get; init; }
        /// <summary>Whether a library logo asset is present.</summary>
        public bool HasLibraryLogo { get; init; }

        private static readonly string[] Mapped =
        { "type", "name", "clienticon", "original_release_date", "steam_release_date" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfoCommon(IReadOnlyDictionary<string, object?> extra) : base(extra) { }

        protected override bool TryGetMapped(string key, out object? value)
        {
            switch (key.ToLowerInvariant())
            {
                case "type": value = Type; return true;
                case "name": value = Name; return true;
                case "clienticon": value = ClientIcon; return true;
                case "original_release_date": value = OriginalReleaseDate; return true;
                case "steam_release_date": value = SteamReleaseDate; return true;
                default: value = null; return false;
            }
        }

        /// <summary>
        /// Builds the strongly-typed <c>common</c> node. <paramref name="extended"/> is supplied so the developer /
        /// publisher fallback (used when the <c>associations</c> list is absent) is preserved.
        /// </summary>
        public static SteamAppInfoCommon? From(BVPropertyCollection? common, BVPropertyCollection? extended = null)
        {
            if (common is null)
                return null;

            var developers = new List<string>();
            var publishers = new List<string>();
            if (GetSection(common, "associations") is BVPropertyCollection associations)
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
            BVPropertyCollection? assets = GetSection(common, "library_assets");
            string? clientIcon = common["clienticon"]?.GetValue<string>();

            return new SteamAppInfoCommon(BuildExtra(common, Mapped))
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
            };
        }
    }

    /// <summary>
    /// Strongly-typed view of the <c>extended</c> section of an app's appinfo.vdf record, with every non-elevated
    /// key preserved in <see cref="BvdfNode.Extra"/>.
    /// </summary>
    public sealed class SteamAppInfoExtended : BvdfNode
    {
        [DataMember(Name = "developer")]
        public string? Developer { get; init; }
        [DataMember(Name = "publisher")]
        public string? Publisher { get; init; }

        private static readonly string[] Mapped = { "developer", "publisher" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfoExtended(IReadOnlyDictionary<string, object?> extra) : base(extra) { }

        protected override bool TryGetMapped(string key, out object? value)
        {
            switch (key.ToLowerInvariant())
            {
                case "developer": value = Developer; return true;
                case "publisher": value = Publisher; return true;
                default: value = null; return false;
            }
        }

        /// <summary>Builds the strongly-typed <c>extended</c> node, or null when the section is absent.</summary>
        public static SteamAppInfoExtended? From(BVPropertyCollection? extended)
        {
            if (extended is null)
                return null;
            return new SteamAppInfoExtended(BuildExtra(extended, Mapped))
            {
                Developer = extended["developer"]?.GetValue<string>(),
                Publisher = extended["publisher"]?.GetValue<string>(),
            };
        }
    }

    /// <summary>
    /// A strongly-typed view of one app record from the Steam client appcache (appinfo.vdf).
    ///
    /// A record is <b>not</b> itself a BVDF section: it pairs the <b>binary</b> chunk-header fields
    /// (appid, state, checksum, ...) with the chunk's <b>BVDF</b> payload, the <c>data</c> field read as a
    /// <see cref="BvdfNode"/> (<see cref="Data"/>, a JObject-equivalent root of the BVDF tree). The standardized
    /// <c>appinfo.common</c> / <c>appinfo.extended</c> sections are exposed as strongly-typed Bvdf nodes
    /// (<see cref="Common"/> / <see cref="Extended"/>), and <c>appinfo.dependencies</c> as a generic node.
    /// Every key of the BVDF payload is still reachable (recursively) through <see cref="BvdfNode.Extra"/> —
    /// nothing Valve (or a custom appinfo entry) writes is ever dropped. Produce instances through
    /// <see cref="SteamAppInfoDataFile.GetApps"/> or <see cref="From"/>.
    /// </summary>
    public sealed class SteamAppInfo
    {
        // -- binary chunk-header fields (read from the file, not from the BVDF payload) ---------------------------
        /// <summary>Steam App ID.</summary>
        public UInt32 AppId { get; init; }
        /// <summary>Per-app state flags from the appcache.</summary>
        public UInt32 State { get; init; }
        /// <summary>Unix timestamp of the last update.</summary>
        public UInt32 LastUpdate { get; init; }
        /// <summary>Per-app access token.</summary>
        public UInt64 AccessToken { get; init; }
        /// <summary>20-byte content checksum.</summary>
        public byte[] Checksum { get; init; } = Array.Empty<byte>();
        /// <summary>Last change number for this app's data.</summary>
        public UInt32 LastChangeNumber { get; init; }

        // -- the BVDF payload (the chunk's `data` field) -------------------------------------------------------------
        /// <summary>
        /// The <c>data</c> field as a lossless generic Bvdf node — the root of the BVDF tree (a JObject-equivalent).
        /// Its <c>appinfo</c> child holds <c>common</c> / <c>extended</c> / <c>dependencies</c>.
        /// </summary>
        [DataMember(Name = "data")]
        public BvdfNode? Data { get; init; }

        /// <summary>The strongly-typed <c>appinfo.common</c> section, or null when the app has no <c>common</c> block.</summary>
        public SteamAppInfoCommon? Common { get; init; }

        /// <summary>The strongly-typed <c>appinfo.extended</c> section, or null when absent.</summary>
        public SteamAppInfoExtended? Extended { get; init; }

        /// <summary>The <c>appinfo.dependencies</c> section as a generic Bvdf node, or null when absent.</summary>
        public BvdfNode? Dependencies { get; init; }

        // -- shortcuts / derived -------------------------------------------------------------------------------------
        /// <summary>App type (shortcut into <see cref="Common"/>), lower-cased. Null when unavailable.</summary>
        public string? Type => Common?.Type;

        /// <summary>Display name (shortcut into <see cref="Common"/>).</summary>
        public string? Name => Common?.Name;

        /// <summary>Absolute on-disk icon path, or null when the app has no client icon / Steam install path.</summary>
        public string? IconPath { get; init; }

        /// <summary>
        /// Builds a <see cref="SteamAppInfo"/> from a raw appinfo.vdf chunk. Returns null when the chunk's
        /// BVDF payload has no <c>common</c> section (such entries are dropped by the filesystem app source).
        /// </summary>
        public static SteamAppInfo? From(SteamAppInfoDataFile.SteamAppInfoDataFileChunk chunk)
        {
            BVPropertyCollection? data = chunk?.data;
            if (data is null)
                return null;

            BVPropertyCollection? appinfo = data["appinfo"] as BVPropertyCollection;
            BVPropertyCollection? common = appinfo?["common"] as BVPropertyCollection;
            BVPropertyCollection? extended = appinfo?["extended"] as BVPropertyCollection;
            if (common is null)
                return null;

            string? clientIcon = common["clienticon"]?.GetValue<string>();
            string? installPath = SteamProcessInfo.SteamInstallPath;

            return new SteamAppInfo
            {
                AppId = chunk.AppID,
                State = chunk.State,
                LastUpdate = chunk.LastUpdate,
                AccessToken = chunk.AccessToken,
                Checksum = chunk.Checksum ?? Array.Empty<byte>(),
                LastChangeNumber = chunk.LastChangeNumber,
                Data = BvdfNode.From(data),
                Common = SteamAppInfoCommon.From(common, extended),
                Extended = SteamAppInfoExtended.From(extended),
                Dependencies = appinfo?["dependencies"] as BVPropertyCollection is { } deps
                    ? BvdfNode.From(deps)
                    : null,
                IconPath = (string.IsNullOrWhiteSpace(clientIcon) || installPath is null)
                    ? null
                    : Path.Combine(installPath, "steam", "games", clientIcon + ".ico"),
            };
        }
    }
}