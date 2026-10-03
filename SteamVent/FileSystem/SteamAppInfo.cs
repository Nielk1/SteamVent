using System;
using System.Collections.Generic;
using System.IO;
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
        public string? Developer { get; init; }
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
    /// A strongly-typed, <i>lossless</i> view of one app record from the Steam client appcache (appinfo.vdf),
    /// mapping the binary VDF chunk onto a clean POCO so callers need not touch <see cref="BVPropertyCollection"/>.
    /// Produce instances through <see cref="SteamAppInfoDataFile.GetApps"/> or <see cref="From"/>.
    ///
    /// The record is available in three complementary forms: the strongly-typed properties here (plus
    /// <see cref="Common"/> / <see cref="Extended"/>), a complete generic node in <see cref="AppInfo"/> /
    /// <see cref="Dependencies"/> (every key preserved), and the raw BVDF in
    /// <see cref="SteamAppInfoDataFile.GetSteamAppInfoDataFile"/>'s chunk. Any key not yet standardized lives in
    /// <see cref="BvdfNode.Extra"/>; see <see cref="BvdfNode"/> for the elevation convention and the
    /// <see cref="BvdfNode.this[string]"/> indexer.
    /// </summary>
    public sealed class SteamAppInfo : BvdfNode
    {
        public UInt32 AppId { get; init; }
        public UInt32 State { get; init; }
        public UInt32 LastUpdate { get; init; }
        public UInt64 AccessToken { get; init; }
        public byte[] Checksum { get; init; } = Array.Empty<byte>();
        public UInt32 LastChangeNumber { get; init; }

        public SteamAppInfoCommon? Common { get; init; }
        public SteamAppInfoExtended? Extended { get; init; }

        /// <summary>The complete <c>appinfo</c> section as a lossless generic node (every key preserved).</summary>
        public BvdfNode? AppInfo { get; init; }

        /// <summary>The <c>appinfo.dependencies</c> section as a generic node, or null when absent.</summary>
        public BvdfNode? Dependencies { get; init; }

        /// <summary>App type (shortcut into <see cref="Common"/>), lower-cased. Null when unavailable.</summary>
        public string? Type => Common?.Type;

        /// <summary>Display name (shortcut into <see cref="Common"/>).</summary>
        public string? Name => Common?.Name;

        /// <summary>Absolute on-disk icon path, or null when the app has no client icon / Steam install path.</summary>
        public string? IconPath { get; init; }

        private static readonly string[] Mapped =
        { "appid", "state", "lastupdate", "accesstoken", "checksum", "lastchangenumber", "appinfo" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfo(IReadOnlyDictionary<string, object?> extra) : base(extra) { }

        protected override bool TryGetMapped(string key, out object? value)
        {
            switch (key.ToLowerInvariant())
            {
                case "appid": value = AppId; return true;
                case "state": value = State; return true;
                case "lastupdate": value = LastUpdate; return true;
                case "accesstoken": value = AccessToken; return true;
                case "checksum": value = Checksum; return true;
                case "lastchangenumber": value = LastChangeNumber; return true;
                case "appinfo": value = AppInfo; return true;
                default: value = null; return false;
            }
        }

        /// <summary>
        /// Builds a <see cref="SteamAppInfo"/> from a raw appinfo.vdf chunk. Returns null when the chunk has no
        /// <c>common</c> section (such entries are dropped by the filesystem app source).
        /// </summary>
        public static SteamAppInfo? From(SteamAppInfoDataFile.SteamAppInfoDataFileChunk chunk)
        {
            BVPropertyCollection? data = chunk?.data;
            if (data is null)
                return null;

            BVPropertyCollection? appinfo = GetSection(data, "appinfo");
            BVPropertyCollection? common = GetSection(appinfo, "common");
            BVPropertyCollection? extended = GetSection(appinfo, "extended");
            if (common is null)
                return null;

            string? clientIcon = common["clienticon"]?.GetValue<string>();
            string? installPath = SteamProcessInfo.SteamInstallPath;

            return new SteamAppInfo(BuildExtra(data, Mapped))
            {
                AppId = chunk.AppID,
                State = chunk.State,
                LastUpdate = chunk.LastUpdate,
                AccessToken = chunk.AccessToken,
                Checksum = chunk.Checksum ?? Array.Empty<byte>(),
                LastChangeNumber = chunk.LastChangeNumber,
                Common = SteamAppInfoCommon.From(common, extended),
                Extended = SteamAppInfoExtended.From(extended),
                AppInfo = appinfo is null ? null : BvdfNode.From(appinfo),
                Dependencies = GetSection(appinfo, "dependencies") is BVPropertyCollection deps
                    ? BvdfNode.From(deps)
                    : null,
                IconPath = (string.IsNullOrWhiteSpace(clientIcon) || installPath is null)
                    ? null
                    : Path.Combine(installPath, "steam", "games", clientIcon + ".ico"),
            };
        }
    }
}