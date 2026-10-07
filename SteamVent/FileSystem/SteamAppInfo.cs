using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SteamVent.Core.BVdf;

namespace SteamVent.FileSystem
{
    /// <summary>
    /// Strongly-typed view of the <c>appinfo</c> section of one app's appinfo.vdf record (the chunk's
    /// <c>data.appinfo</c> block). <see cref="Common"/> and <see cref="Extended"/> are elevated to strongly-typed
    /// nodes; every other key (e.g. <c>appid</c>, <c>public_only</c>, <c>dependencies</c>) is preserved in
    /// <see cref="BvdfNode.Extra"/> and stays reachable through <see cref="BvdfNode.this[string]"/>.
    ///
    /// The binary chunk-header fields (<c>AppID</c>, <c>State</c>, <c>Checksum</c>, ...) intentionally live on the
    /// owning <see cref="SteamAppInfoDataFile.SteamAppInfoDataFileChunk"/>, not here: this node is only the BVDF payload.
    /// </summary>
    public sealed class SteamAppInfo : BvdfNode
    {
        private SteamAppInfoCommon? _common;
        private SteamAppInfoExtended? _extended;

        /// <summary>The strongly-typed <c>common</c> section, or null when the app has no <c>common</c> block.</summary>
        [DataMember(Name = "common")]
        public SteamAppInfoCommon? Common => _common ??= Section?["common"] is BVPropertyCollection c ? new SteamAppInfoCommon(c) : null;

        /// <summary>The strongly-typed <c>extended</c> section, or null when absent.</summary>
        [DataMember(Name = "extended")]
        public SteamAppInfoExtended? Extended => _extended ??= Section?["extended"] is BVPropertyCollection e ? new SteamAppInfoExtended(e) : null;

        private static readonly string[] Mapped = { "common", "extended" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfo(BVPropertyCollection section) : base(section) { }

        protected override bool TryGetMapped(string key, out object? value)
        {
            switch (key.ToLowerInvariant())
            {
                case "common": value = Common; return true;
                case "extended": value = Extended; return true;
                default: value = null; return false;
            }
        }

        /// <summary>Builds the strongly-typed node over an <c>appinfo</c> section, or null when the section is absent.</summary>
        public static SteamAppInfo? From(BVPropertyCollection? appinfo) => appinfo is null ? null : new SteamAppInfo(appinfo);

        // -- reduction phase: these shortcut into Common / augment with the install path; kept available to restore --
        // /// <summary>App type (shortcut into <see cref="Common"/>), lower-cased. Null when unavailable.</summary>
        // public string? Type => Common?.Type;
        // /// <summary>Display name (shortcut into <see cref="Common"/>).</summary>
        // public string? Name => Common?.Name;
        // /// <summary>Absolute on-disk icon path, or null when the app has no client icon / Steam install path.</summary>
        // public string? IconPath { get; }
    }

    /// <summary>
    /// Strongly-typed view of the <c>common</c> section of an app's appinfo.vdf record. Standardized keys are elevated
    /// to lazy, strongly-typed properties (read straight from the underlying <see cref="BVToken"/> at access time);
    /// every other key is preserved in <see cref="BvdfNode.Extra"/>.
    /// </summary>
    public sealed class SteamAppInfoCommon : BvdfNode
    {
        /// <summary>App type (e.g. "game", "demo", "dlc"), lower-cased. Null/empty when unset.</summary>
        [DataMember(Name = "type")]
        public string? Type => Section?["type"]?.GetValue<string>()?.ToLowerInvariant();

        /// <summary>Display name.</summary>
        [DataMember(Name = "name")]
        public string? Name => Section?["name"]?.GetValue<string>();

        /// <summary>Raw client-icon key (no directory / extension).</summary>
        [DataMember(Name = "clienticon")]
        public string? ClientIcon => Section?["clienticon"]?.GetValue<string>();

        /// <summary>Original (non-Steam) release date, when present.</summary>
        [DataMember(Name = "original_release_date")]
        public DateTime? OriginalReleaseDate => FromUnix(Section?["original_release_date"]);

        /// <summary>Steam release date, when present.</summary>
        [DataMember(Name = "steam_release_date")]
        public DateTime? SteamReleaseDate => FromUnix(Section?["steam_release_date"]);

        private static DateTime? FromUnix(BVToken? token)
        {
            long? ts = token?.GetValue<long>();
            return ts.HasValue ? DateTimeOffset.FromUnixTimeSeconds(ts.Value).UtcDateTime : null;
        }

        private static readonly string[] Mapped =
        { "type", "name", "clienticon", "original_release_date", "steam_release_date" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfoCommon(BVPropertyCollection section) : base(section) { }

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

        // -- reduction phase: these dive into `associations` / `library_assets` or the `extended` block; kept available to restore --
        // /// <summary>Developer names (the "associations" list, with the extended block as fallback).</summary>
        // public IReadOnlyList<string> Developers { get; }
        // /// <summary>Publisher names (the "associations" list, with the extended block as fallback).</summary>
        // public IReadOnlyList<string> Publishers { get; }
        // /// <summary>Whether a library capsule asset is present.</summary>
        // public bool HasLibraryCapsule { get; }
        // /// <summary>Whether a library hero asset is present.</summary>
        // public bool HasLibraryHero { get; }
        // /// <summary>Whether a library logo asset is present.</summary>
        // public bool HasLibraryLogo { get; }
    }

    /// <summary>
    /// Strongly-typed view of the <c>extended</c> section of an app's appinfo.vdf record, with every non-elevated key
    /// preserved in <see cref="BvdfNode.Extra"/>.
    /// </summary>
    public sealed class SteamAppInfoExtended : BvdfNode
    {
        /// <summary>Developer name.</summary>
        [DataMember(Name = "developer")]
        public string? Developer => Section?["developer"]?.GetValue<string>();

        /// <summary>Publisher name.</summary>
        [DataMember(Name = "publisher")]
        public string? Publisher => Section?["publisher"]?.GetValue<string>();

        private static readonly string[] Mapped = { "developer", "publisher" };
        public override IReadOnlyCollection<string> MappedKeys => Mapped;

        internal SteamAppInfoExtended(BVPropertyCollection section) : base(section) { }

        protected override bool TryGetMapped(string key, out object? value)
        {
            switch (key.ToLowerInvariant())
            {
                case "developer": value = Developer; return true;
                case "publisher": value = Publisher; return true;
                default: value = null; return false;
            }
        }
    }
}