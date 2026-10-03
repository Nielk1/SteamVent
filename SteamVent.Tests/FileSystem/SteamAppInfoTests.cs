using System;
using System.Collections.Generic;
using SteamVent.Core.BVdf;
using SteamVent.FileSystem;
using Xunit;

namespace SteamVent.Tests.FileSystem
{
    /// <summary>
    /// Verifies the strongly-typed <see cref="SteamAppInfo"/> model is a <b>lossless</b> view of the raw BVDF data:
    /// every key of a raw section is either elevated to a strongly-typed property or preserved in
    /// <see cref="BvdfNode.Extra"/>, the string indexer reads from both, and non-standard data is moved into the
    /// dictionary (with nested objects / arrays preserved).
    /// </summary>
    public class SteamAppInfoTests
    {
        /// <summary>Builds a realistic appinfo.vdf chunk payload (raw BVDF) with typed, custom, and nested data.</summary>
        private static (SteamAppInfoDataFile.SteamAppInfoDataFileChunk chunk, BVPropertyCollection data) BuildSampleChunk()
        {
            // common.associations (array of {type,name})
            var developer = new BVPropertyCollection();
            developer.Add("type", "developer");
            developer.Add("name", "Valve");
            var publisher = new BVPropertyCollection();
            publisher.Add("type", "publisher");
            publisher.Add("name", "Valve");
            var associations = new BVPropertyCollection();
            associations.Add("0", developer);
            associations.Add("1", publisher);

            // common.library_assets
            var library = new BVPropertyCollection();
            library.Add("library_capsule", "1");
            library.Add("library_hero", "1");

            // an unknown custom object (should become a BvdfNode)
            var customObject = new BVPropertyCollection();
            customObject.Add("inner", "value");
            customObject.Add("num", 42);

            // common
            var common = new BVPropertyCollection();
            common.Add("type", "GAME");
            common.Add("name", "Test Game");
            common.Add("clienticon", "testgame");
            common.Add("original_release_date", 1285977600);
            common.Add("steam_release_date", 1285977601);
            common.Add("associations", associations);
            common.Add("library_assets", library);
            common.Add("oslist", "windows");           // non-standard -> Extra
            common.Add("custom_object", customObject); // non-standard object -> Extra
            common.Add("custom_count", 7);             // non-standard scalar -> Extra

            // extended
            var extended = new BVPropertyCollection();
            extended.Add("developer", "Valve");
            extended.Add("publisher", "Valve");
            extended.Add("is_free", "0");              // non-standard -> Extra
            extended.Add("custom_flag", "on");         // non-standard -> Extra

            // dependencies (array)
            var depIds = new BVPropertyCollection();
            depIds.Add("0", "570");
            depIds.Add("1", "440");
            var dependencies = new BVPropertyCollection();
            dependencies.Add("appid", depIds);

            // appinfo
            var appinfo = new BVPropertyCollection();
            appinfo.Add("appid", 12345);
            appinfo.Add("lastchange", "1690000000");
            appinfo.Add("common", common);
            appinfo.Add("extended", extended);
            appinfo.Add("dependencies", dependencies);

            // chunk payload (data)
            var data = new BVPropertyCollection();
            data.Add("appid", 12345);
            data.Add("state", 1);
            data.Add("lastupdate", 1690000000);
            data.Add("accesstoken", "token-abc");
            data.Add("checksum", "c1c2c3");
            data.Add("lastchangenumber", 42);
            data.Add("appinfo", appinfo);

            var chunk = new SteamAppInfoDataFile.SteamAppInfoDataFileChunk(
                AppID: 12345,
                State: 1,
                LastUpdate: 1690000000,
                AccessToken: 0x1122334455667788UL,
                Checksum: new byte[] { 1, 2, 3 },
                LastChangeNumber: 42,
                data: data);
            return (chunk, data);
        }

        [Fact]
        public void Strongly_typed_node_preserves_every_raw_key()
        {
            var (chunk, data) = BuildSampleChunk();
            SteamAppInfo app = SteamAppInfo.From(chunk)!;
            Assert.NotNull(app);

            BVPropertyCollection appinfo = (BVPropertyCollection)data["appinfo"];
            BVPropertyCollection common = (BVPropertyCollection)appinfo["common"];
            BVPropertyCollection extended = (BVPropertyCollection)appinfo["extended"];
            BVPropertyCollection dependencies = (BVPropertyCollection)appinfo["dependencies"];

            // Losslessness: every raw key is exposed by the strongly-typed node (elevated OR in Extra).
            AssertComplete(app, data);
            AssertComplete(app.Common, common);
            AssertComplete(app.Extended, extended);
            AssertComplete(app.Dependencies, dependencies);

            // No key is both elevated and duplicated inside Extra.
            AssertNoDuplication(app);
            AssertNoDuplication(app.Common);
            AssertNoDuplication(app.Extended);
        }

        [Fact]
        public void Typed_values_are_parsed_correctly()
        {
            var (chunk, _) = BuildSampleChunk();
            SteamAppInfo app = SteamAppInfo.From(chunk)!;
            Assert.NotNull(app);

            // header (from the chunk envelope)
            Assert.Equal(12345u, app.AppId);
            Assert.Equal(1u, app.State);
            Assert.Equal(0x1122334455667788UL, app.AccessToken);
            Assert.Equal(new byte[] { 1, 2, 3 }, app.Checksum);

            // common (typed)
            Assert.Equal("game", app.Common!.Type); // lower-cased
            Assert.Equal("Test Game", app.Common.Name);
            Assert.Equal("testgame", app.Common.ClientIcon);
            Assert.Equal(new[] { "Valve" }, app.Common.Developers);
            Assert.Equal(new[] { "Valve" }, app.Common.Publishers);
            Assert.True(app.Common.HasLibraryCapsule);
            Assert.True(app.Common.HasLibraryHero);
            Assert.False(app.Common.HasLibraryLogo);
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1285977600).UtcDateTime, app.Common.OriginalReleaseDate);

            // extended (typed)
            Assert.Equal("Valve", app.Extended!.Developer);
            Assert.Equal("Valve", app.Extended.Publisher);

            // root shortcuts
            Assert.Equal("Test Game", app.Name);
            Assert.Equal("game", app.Type);
        }

        [Fact]
        public void Non_standard_data_is_moved_into_the_dictionary()
        {
            var (chunk, _) = BuildSampleChunk();
            SteamAppInfo app = SteamAppInfo.From(chunk)!;
            Assert.NotNull(app);

            // scalars
            Assert.Equal("windows", app.Common!.Extra["oslist"]);
            Assert.Equal(7, (int)(object)app.Common.Extra["custom_count"]);

            // a nested unknown object becomes a BvdfNode
            BvdfNode custom = Assert.IsType<BvdfNode>(app.Common.Extra["custom_object"]);
            Assert.Equal("value", (string)(object)custom.Extra["inner"]);
            Assert.Equal(42, (int)(object)custom.Extra["num"]);

            // extended non-standard keys
            Assert.Equal("0", app.Extended!.Extra["is_free"]);
            Assert.Equal("on", app.Extended.Extra["custom_flag"]);

            // dependencies array is preserved as a list
            var depList = Assert.IsType<List<object?>>(app.Dependencies!.Extra["appid"]);
            Assert.Equal(2, depList.Count);
            Assert.Equal("570", (string)(object)depList[0]);
            Assert.Equal("440", (string)(object)depList[1]);
        }

        [Fact]
        public void String_indexer_reads_elevated_and_extra_and_nested()
        {
            var (chunk, _) = BuildSampleChunk();
            SteamAppInfo app = SteamAppInfo.From(chunk)!;
            Assert.NotNull(app);

            // elevated (strongly-typed) keys
            Assert.Equal(12345u, (UInt32)(object)app["appid"]);
            Assert.Equal("Test Game", (string)(object)app.Common!["name"]);
            Assert.Equal("game", (string)(object)app.Common["type"]);

            // extra (dictionary) keys
            Assert.Equal("windows", (string)(object)app.Common["oslist"]);
            Assert.Equal("on", (string)(object)app.Extended!["custom_flag"]);

            // nested generic node reached through the indexer
            Assert.Equal("value", (string)(object)app.Common["custom_object"]["inner"]);

            // appinfo is exposed as a generic (lossless) node
            BvdfNode info = Assert.IsType<BvdfNode>(app["appinfo"]);
            Assert.IsType<BvdfNode>((object)info["common"]);
            Assert.Equal("1690000000", (string)(object)info["lastchange"]);

            // unknown key -> null
            Assert.Null((object)app.Common["does_not_exist"]);
        }

        [Fact]
        public void Developers_and_publishers_fall_back_to_extended_when_associations_are_empty()
        {
            var extended = new BVPropertyCollection();
            extended.Add("developer", "Solo Dev");
            extended.Add("publisher", "Solo Pub");
            var common = new BVPropertyCollection();
            common.Add("type", "GAME");
            common.Add("name", "Fallback App");

            var appinfo = new BVPropertyCollection();
            appinfo.Add("common", common);
            appinfo.Add("extended", extended);
            var data = new BVPropertyCollection();
            data.Add("appinfo", appinfo);
            var chunk = new SteamAppInfoDataFile.SteamAppInfoDataFileChunk(777, 1, 0, 0, new byte[20], 0, data);

            SteamAppInfo app = SteamAppInfo.From(chunk)!;
            Assert.NotNull(app);
            Assert.Equal(new[] { "Solo Dev" }, app.Common!.Developers);
            Assert.Equal(new[] { "Solo Pub" }, app.Common.Publishers);
            Assert.Equal("Solo Dev", app.Extended!.Developer);
            Assert.Equal("Solo Pub", app.Extended.Publisher);
        }

        // ---- helpers ----

        private static void AssertComplete(BvdfNode? node, BVPropertyCollection section)
        {
            Assert.NotNull(node);
            foreach (BVProperty p in section.Properties)
                Assert.True(node!.HasKey(p.Key), $"'{p.Key}' is not exposed by the strongly-typed node");
        }

        private static void AssertNoDuplication(BvdfNode? node)
        {
            Assert.NotNull(node);
            foreach (string k in node!.MappedKeys)
                Assert.False(node.Extra.ContainsKey(k), $"'{k}' is elevated AND present in Extra");
        }
    }
}