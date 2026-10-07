using SteamVent.Core.BVdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.FileSystem
{
    //http://translate.google.com/translate?hl=en&sl=ru&u=http://forum.csmania.ru/viewtopic.php%3Ff%3D38%26t%3D30773
    //https://habrahabr.ru/post/268921/
    public class SteamAppInfoDataFile
    {
        public static string GetAppCacheAppInfoFile()
        {
            uint userid = SteamProcessInfo.CurrentUserID;

            string installPath = SteamProcessInfo.SteamInstallPath;

            if (userid > 0)
            {
                string appInfoFile = Path.Combine(installPath, @"appcache", @"appinfo.vdf");
                if (File.Exists(appInfoFile))
                    return appInfoFile;
            }

            return null;
        }

        public static SteamAppInfoDataFile GetSteamAppInfoDataFile(string? path = null)
        {
            return SteamAppInfoDataFile.Read(path ?? GetAppCacheAppInfoFile());
        }

        /// <summary>
        /// Reads the appcache (appinfo.vdf) and returns every chunk that carries an <c>appinfo.common</c> block.
        /// This is the filesystem-section function for app enumeration: it only reads the file system and never
        /// touches Steamworks, so it works whether or not Steam is running. Callers use the binary envelope
        /// (<see cref="SteamAppInfoDataFileChunk.AppID"/> etc.) together with the strongly-typed
        /// <see cref="SteamAppInfoDataFileChunk.Data"/> payload.
        /// </summary>
        public static IReadOnlyList<SteamAppInfoDataFileChunk> GetApps()
        {
            var result = new List<SteamAppInfoDataFileChunk>();
            SteamAppInfoDataFile? file = GetSteamAppInfoDataFile();
            if (file?.Chunks == null)
                return result;
            foreach (SteamAppInfoDataFileChunk chunk in file.Chunks)
                if (chunk.Data?.Common != null)
                    result.Add(chunk);
            return result;
        }





        public byte Version1;
        public UInt16 Type; // 0x4456 ('DV')
        public byte Version2;
        public UInt32 Version3;
        public List<SteamAppInfoDataFileChunk> Chunks;

        public SteamAppInfoDataFile(byte Version1, UInt16 Type, byte Version2, UInt32 Version3, List<SteamAppInfoDataFileChunk> chunks)
        {
            this.Version1 = Version1;
            this.Type = Type;
            this.Version2 = Version2;
            this.Version3 = Version3;
            this.Chunks = chunks;
        }

        public class SteamAppInfoDataFileChunk
        {
            // -- binary envelope (the file's binary layout, not the BVDF payload) ----------------------------------
            public UInt32 AppID;
            public UInt32 State;
            public UInt32 LastUpdate;
            public UInt64 AccessToken;
            public byte[] Checksum;
            public UInt32 LastChangeNumber;

            private readonly BVPropertyCollection _rawData;
            private SteamAppInfo? _data;

            /// <summary>
            /// The strongly-typed <c>appinfo</c> payload (the chunk's <c>data.appinfo</c> block), wrapping the raw
            /// BVDF so <see cref="Data.Common"/> / <see cref="Data.Extended"/> overlay the raw <c>common</c> /
            /// <c>extended</c> sections and every other key stays reachable through <see cref="BvdfNode.Extra"/>.
            /// Null when the chunk's BVDF payload has no <c>appinfo</c> block.
            /// </summary>
            public SteamAppInfo? Data => _data ??= _rawData?["appinfo"] is BVPropertyCollection a ? new SteamAppInfo(a) : null;

            public SteamAppInfoDataFileChunk(UInt32 AppID, UInt32 State, UInt32 LastUpdate, UInt64 AccessToken, byte[] Checksum, UInt32 LastChangeNumber, BVPropertyCollection data)
            {
                this.AppID = AppID;
                this.State = State;
                this.LastUpdate = LastUpdate;
                this.AccessToken = AccessToken;
                this.Checksum = Checksum;
                this.LastChangeNumber = LastChangeNumber;
                this._rawData = data;
            }
        }

        public static SteamAppInfoDataFile Read(string steamAppInfoFilePath)
        {
            if (!File.Exists(steamAppInfoFilePath))
                return null;
            using (FileStream stream = File.OpenRead(steamAppInfoFilePath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                List<SteamAppInfoDataFileChunk> chunks = new List<SteamAppInfoDataFileChunk>();
                byte Version1 = reader.ReadByte();
                UInt16 Type = reader.ReadUInt16();
                byte Version2 = reader.ReadByte();
                UInt32 Version3 = reader.ReadUInt32();
                //while(reader.BaseStream.Position < reader.BaseStream.Length)
                for (; ; )
                {
                    UInt32 AppID = reader.ReadUInt32();
                    if (AppID == 0) break;
                    UInt32 DataSize = reader.ReadUInt32();
                    long startPos = reader.BaseStream.Position;
                    //Console.WriteLine($"Expected End Position: {(startPos + DataSize):X8}");

                    UInt32 State = reader.ReadUInt32();
                    UInt32 LastUpdate = reader.ReadUInt32();
                    UInt64 AccessToken = reader.ReadUInt64();
                    byte[] Checksum = reader.ReadBytes(20);
                    UInt32 LastChangeNumber = reader.ReadUInt32();

                    BVPropertyCollection Data = BVdfFile.ReadPropertyArray(reader);
                    //long endPos = reader.BaseStream.Position;
                    if (reader.BaseStream.Position != (startPos + DataSize))
                    {
                        Console.WriteLine("appinfo.vdf chunk data size wrong, adjusting stream position");
                        reader.BaseStream.Seek(startPos + DataSize, SeekOrigin.Begin);
                    }
                    //Console.WriteLine($"*Expected End Position: {(startPos + DataSize):X8}");
                    //Console.WriteLine($"End Position: {(endPos):X8}");

                    SteamAppInfoDataFileChunk Chunk = new SteamAppInfoDataFileChunk(AppID, State, LastUpdate, AccessToken, Checksum, LastChangeNumber, Data);
                    chunks.Add(Chunk);
                }
                return new SteamAppInfoDataFile(Version1, Type, Version2, Version3, chunks);
            }
        }
    }
}
