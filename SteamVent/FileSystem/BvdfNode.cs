using System;
using System.Collections.Generic;
using System.Linq;
using SteamVent.Core.BVdf;

namespace SteamVent.FileSystem
{
    /// <summary>
    /// A lazy, lossless view over a single VDF (BVDF) section, and the shared base class for every strongly-typed
    /// node in this assembly (see <see cref="SteamAppInfo"/>, <see cref="SteamAppInfoCommon"/>,
    /// <see cref="SteamAppInfoExtended"/>).
    ///
    /// A <b>node is one section</b>: it wraps the raw <see cref="BVPropertyCollection"/> (<see cref="Section"/>)
    /// and preserves <b>every</b> key/value of it. A derived type "elevates" the keys it has standardized into
    /// strongly-typed properties (listed in <see cref="MappedKeys"/>) and the rest are kept in <see cref="Extra"/>.
    /// That makes the strongly-typed form a lossless view of the raw BVDF data: nothing Valve (or a custom appinfo
    /// entry) writes to the file is ever dropped, even if it is not standardized yet.
    ///
    /// The node <b>lazily</b> materializes CLR values: scalars convert to their primitive, a nested section becomes
    /// a child <see cref="BvdfNode"/>, and a VDF array (keys "0".."n-1") becomes a <see cref="List{T}"/>. Nothing
    /// deep in the tree is boxed until actually read, so merely wrapping a large appcache section is cheap.
    ///
    /// <see cref="BvdfNode"/> is also used directly as the generic "unknown object" node: an instance with no
    /// elevated keys is just its <see cref="Extra"/> dictionary, reached through the same surface
    /// (<see cref="this[string]"/>, <see cref="Extra"/>, <see cref="Entries()"/>) as the typed nodes.
    ///
    /// A unified string-key view is provided by the <see cref="this[string]"/> indexer: it returns the strongly-typed
    /// value for an elevated key, otherwise the lazily-converted raw value from the section, otherwise null. Because a
    /// node's values are heterogeneous (string, integer, floating-point, boolean, nested node, array) the indexer is
    /// declared <c>dynamic</c>. Standardization is incremental: when a key that currently lives in <see cref="Extra"/>
    /// becomes standardized it is "elevated" to a strongly-typed property and added to that type's
    /// <see cref="MappedKeys"/>; from then on it is no longer part of <see cref="Extra"/>.
    /// </summary>
    public class BvdfNode
    {
        private readonly BVPropertyCollection _section;
        private IReadOnlyDictionary<string, object?>? _extra;

        /// <summary>Creates a node over an empty section.</summary>
        public BvdfNode() : this(new BVPropertyCollection()) { }

        internal BvdfNode(BVPropertyCollection section)
        {
            _section = section ?? new BVPropertyCollection();
        }

        /// <summary>
        /// The raw underlying BVDF section - the single source of truth (the "weak" representation) that this node
        /// wraps. Exposed so callers that need the raw <see cref="BVPropertyCollection"/> API can reach it.
        /// </summary>
        public BVPropertyCollection Section => _section;

        /// <summary>
        /// Every non-elevated key/value of the underlying section, in insertion order. Lazily materialized on first
        /// read: scalars convert to their primitive, nested sections become child <see cref="BvdfNode"/>s (themselves
        /// lazy), and VDF arrays become <see cref="List{T}"/>. Keys are matched case-insensitively.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Extra => _extra ??= BuildExtraView();

        /// <summary>The standardized (strongly-typed) keys this node type elevates to properties.</summary>
        public virtual IReadOnlyCollection<string> MappedKeys { get; } = Array.Empty<string>();

        /// <summary>True if <paramref name="key"/> is present, as an elevated property or inside the section.</summary>
        public bool HasKey(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            foreach (string k in MappedKeys)
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            return _section[key] != null;
        }

        /// <summary>
        /// Unified string-key access: the strongly-typed value for an elevated key, else the lazily-converted raw
        /// value from the section, else null. Declared <c>dynamic</c> so callers can operate on heterogeneous values
        /// (including nested <see cref="BvdfNode"/> children) without an explicit cast.
        /// </summary>
        public dynamic? this[string? key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                if (TryGetMapped(key, out object? mapped))
                    return mapped;
                return Convert(_section[key]);
            }
        }

        /// <summary>
        /// True if <paramref name="key"/> is a standardized key this node elevates to a property; sets
        /// <paramref name="value"/> to that property's value (possibly null). Derived types override this to map
        /// their elevated keys onto their strongly-typed properties.
        /// </summary>
        protected virtual bool TryGetMapped(string key, out object? value)
        {
            value = null;
            return false;
        }

        /// <summary>All entries of the node: elevated properties first, then the section's other keys.</summary>
        public IEnumerable<KeyValuePair<string, object?>> Entries()
        {
            foreach (string k in MappedKeys)
            {
                TryGetMapped(k, out object? v);
                yield return new KeyValuePair<string, object?>(k, v);
            }
            foreach (KeyValuePair<string, object?> kv in Extra)
                yield return kv;
        }

        /// <summary>All keys of the node: elevated keys first, then the section's other keys.</summary>
        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string k in MappedKeys)
                    yield return k;
                foreach (KeyValuePair<string, object?> kv in Extra)
                    yield return kv.Key;
            }
        }

        /// <summary>Total number of entries the node exposes (elevated keys + the section's other keys).</summary>
        public int Count => MappedKeys.Count + Extra.Count;

        /// <summary>Creates a generic (no elevated keys) node that losslessly wraps a VDF section.</summary>
        public static BvdfNode From(BVPropertyCollection section) => new BvdfNode(section);

        // -- lazy materialization (shared by every node) -------------------------------------------------------------

        private IReadOnlyDictionary<string, object?> BuildExtraView()
        {
            var exclude = new HashSet<string>(MappedKeys, StringComparer.OrdinalIgnoreCase);
            var extra = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (BVProperty prop in _section.Properties)
            {
                if (exclude.Contains(prop.Key))
                    continue;
                extra[prop.Key] = Convert(prop.Value);
            }
            return extra;
        }

        /// <summary>
        /// Converts a BVDF token to a CLR value: scalars to their primitive, a nested section to a lazy child
        /// <see cref="BvdfNode"/> (whose own contents are materialized only on first read), and a VDF array (keys
        /// "0".."n-1") to a <see cref="List{T}"/>. No deep tree is boxed ahead of time.
        /// </summary>
        protected static object? Convert(BVToken? token)
        {
            if (token is null)
                return null;
            if (token is BVPropertyCollection col)
                return IsArray(col)
                    ? col.Properties.Select(p => Convert(p.Value)).ToList()
                    : new BvdfNode(col);
            return token.GetValue();
        }

        /// <summary>True if <paramref name="col"/> is a VDF array (its keys are exactly "0".."n-1").</summary>
        private static bool IsArray(BVPropertyCollection col)
        {
            int expected = 0;
            foreach (BVProperty p in col.Properties)
            {
                if (!int.TryParse(p.Key, out int k) || k != expected)
                    return false;
                expected++;
            }
            return col.Properties.Count > 0;
        }
    }
}