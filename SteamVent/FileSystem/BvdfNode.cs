using System;
using System.Collections.Generic;
using System.Linq;
using SteamVent.Core.BVdf;

namespace SteamVent.FileSystem
{
    /// <summary>
    /// A node over a single VDF (BVDF) section, and the shared base class for every strongly-typed node in
    /// <see cref="SteamAppInfo"/>.
    ///
    /// A node preserves <b>every</b> key/value of its underlying section. A derived type "elevates" the keys it
    /// has standardized into strongly-typed properties (listed in <see cref="MappedKeys"/>) and the rest are kept,
    /// recursively, in <see cref="Extra"/>. That makes the strongly-typed form a <i>lossless</i> view of the raw
    /// BVDF data - nothing Valve (or a custom appinfo entry) writes to the file is ever dropped, even if it is
    /// not standardized yet.
    ///
    /// <see cref="BvdfNode"/> is also used directly as the generic "unknown object" node: an instance with no
    /// elevated keys is just its <see cref="Extra"/> dictionary, reached through the same surface
    /// (<see cref="this[string]"/>, <see cref="Extra"/>, <see cref="Entries()"/>) as the typed nodes. This is how
    /// unknown objects inside a strongly-typed collection are represented (e.g. <see cref="SteamAppInfo.Data"/>).
    ///
    /// A unified string-key view is provided by the <see cref="this[string]"/> indexer: it returns the
    /// strongly-typed value for an elevated key, otherwise the raw value from <see cref="Extra"/>, otherwise null.
    /// Because a node's values are heterogeneous (string, integer, floating-point, boolean, nested node, array)
    /// the indexer is declared <c>dynamic</c>.
    ///
    /// Standardization is incremental: when a key that currently lives in <see cref="Extra"/> becomes standardized
    /// it is "elevated" to a strongly-typed property on the appropriate derived type and added to that type's
    /// <see cref="MappedKeys"/>; from then on it is no longer part of <see cref="Extra"/>.
    /// </summary>
    public class BvdfNode
    {
        private readonly IReadOnlyDictionary<string, object?> _extra;

        /// <summary>Creates a generic node with no entries.</summary>
        public BvdfNode() : this(null) { }

        internal BvdfNode(IReadOnlyDictionary<string, object?>? extra)
        {
            _extra = extra ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Every non-elevated key/value of the underlying section, in insertion order. This is where non-standard,
        /// custom, and not-yet-elevated data lives. Keys are matched case-insensitively.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Extra => _extra;

        /// <summary>The standardized (strongly-typed) keys this node type elevates to properties.</summary>
        public virtual IReadOnlyCollection<string> MappedKeys { get; } = Array.Empty<string>();

        /// <summary>True if <paramref name="key"/> is present, as an elevated property or inside <see cref="Extra"/>.</summary>
        public bool HasKey(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            foreach (string k in MappedKeys)
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return true;
            return _extra.ContainsKey(key);
        }

        /// <summary>
        /// Unified string-key access: the strongly-typed value for an elevated key, else the raw value from
        /// <see cref="Extra"/>, else null. Declared <c>dynamic</c> so callers can operate on heterogeneous values
        /// (including nested <see cref="BvdfNode"/> children) without an explicit cast.
        /// </summary>
        public dynamic this[string? key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    return null;
                if (TryGetMapped(key, out object? mapped))
                    return mapped;
                if (_extra.TryGetValue(key, out object? raw))
                    return raw;
                return null;
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

        /// <summary>All entries of the node: elevated properties first, then <see cref="Extra"/> entries.</summary>
        public IEnumerable<KeyValuePair<string, object?>> Entries()
        {
            foreach (string k in MappedKeys)
            {
                TryGetMapped(k, out object? v);
                yield return new KeyValuePair<string, object?>(k, v);
            }
            foreach (KeyValuePair<string, object?> kv in _extra)
                yield return kv;
        }

        /// <summary>All keys of the node: elevated keys first, then <see cref="Extra"/> keys.</summary>
        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string k in MappedKeys)
                    yield return k;
                foreach (KeyValuePair<string, object?> kv in _extra)
                    yield return kv.Key;
            }
        }

        /// <summary>Total number of entries the node exposes (elevated keys + <see cref="Extra"/> entries).</summary>
        public int Count => MappedKeys.Count + _extra.Count;

        /// <summary>Creates a generic (no elevated keys) node that losslessly wraps a VDF section.</summary>
        public static BvdfNode From(BVPropertyCollection section) => new BvdfNode(BuildExtra(section));

        // -- normalization helpers (shared by every node) ----------------------------------------------------------

        /// <summary>
        /// Normalizes a BVDF token into a plain CLR value: scalars to their primitive, a VDF array (keys
        /// "0".."n-1") to a <see cref="List{T}"/>, and any other nested section to a child <see cref="BvdfNode"/>.
        /// </summary>
        protected static object? NormalizeToken(BVToken? token)
        {
            if (token is null)
                return null;
            if (token is BVPropertyCollection col)
                return IsArray(col)
                    ? col.Properties.Select(p => NormalizeToken(p.Value)).ToList()
                    : new BvdfNode(BuildExtra(col));
            return token.GetValue();
        }

        /// <summary>
        /// Builds the <see cref="Extra"/> dictionary for <paramref name="section"/> holding every key/value whose
        /// key is <b>not</b> one of <paramref name="elevatedKeys"/>. Values are normalized recursively.
        /// </summary>
        protected static IReadOnlyDictionary<string, object?> BuildExtra(
            BVPropertyCollection section, IEnumerable<string>? elevatedKeys = null)
        {
            var exclude = new HashSet<string>(
                elevatedKeys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var extra = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (BVProperty prop in section.Properties)
            {
                if (exclude.Contains(prop.Key))
                    continue;
                extra[prop.Key] = NormalizeToken(prop.Value);
            }
            return extra;
        }

        /// <summary>Returns the nested section under <paramref name="parent"/>'s <paramref name="key"/>, or null.</summary>
        protected static BVPropertyCollection? GetSection(BVPropertyCollection? parent, string key)
            => parent?[key] as BVPropertyCollection;

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