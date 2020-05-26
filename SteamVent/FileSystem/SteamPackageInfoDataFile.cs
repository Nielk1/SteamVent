using SteamVent.Common.BVdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.FileSystem
{
    public class SteamPackageInfoDataFile
    {
        public static string GetAppCachePackageInfoFile()
        {
            uint userid = SteamProcessInfo.CurrentUserID;

            string installPath = SteamProcessInfo.SteamInstallPath;

            if (userid > 0)
            {
                string appInfoFile = Path.Combine(installPath, @"appcache", @"packageinfo.vdf");
                if (File.Exists(appInfoFile))
                    return appInfoFile;
            }

            return null;
        }
        public static SteamPackageInfoDataFile GetSteamPackageInfoDataFile(string path = null)
        {
            return SteamPackageInfoDataFile.Read(path ?? GetAppCachePackageInfoFile());
        }





        public byte Version1;
        public UInt16 Type; // 0x5556 ('UV')
        public byte Version2;
        public UInt32 Version3;
        public List<SteamPackageInfoDataFileChunk> chunks;

        public SteamPackageInfoDataFile(byte Version1, UInt16 Type, byte Version2, UInt32 Version3, List<SteamPackageInfoDataFileChunk> chunks)
        {
            this.Version1 = Version1;
            this.Type = Type;
            this.Version2 = Version2;
            this.Version3 = Version3;
            this.chunks = chunks;
        }

        public class SteamPackageInfoDataFileChunk
        {
            public UInt32 SubID;
            //public UInt32 State;
            //public UInt32 LastUpdate;
            //public UInt64 AccessToken;
            public byte[] Checksum;
            public UInt32 LastChangeNumber;
            public BVPropertyCollection data;

            public SteamPackageInfoDataFileChunk(UInt32 SubID, /*UInt32 State, UInt32 LastUpdate, UInt64 AccessToken,*/ byte[] Checksum, UInt32 LastChangeNumber, BVPropertyCollection data)
            {
                this.SubID = SubID;
                //this.State = State;
                //this.LastUpdate = LastUpdate;
                //this.AccessToken = AccessToken;
                this.Checksum = Checksum;
                this.LastChangeNumber = LastChangeNumber;
                this.data = data;
            }
        }

        public static SteamPackageInfoDataFile Read(string steamShortcutFilePath)
        {
            using (FileStream stream = File.OpenRead(steamShortcutFilePath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                List<SteamPackageInfoDataFileChunk> Chunks = new List<SteamPackageInfoDataFileChunk>();
                byte Version1 = reader.ReadByte();
                UInt16 Type = reader.ReadUInt16();
                byte Version2 = reader.ReadByte();
                UInt32 Version3 = reader.ReadUInt32();
                //while(reader.BaseStream.Position < reader.BaseStream.Length)
                for (; ; )
                {
                    UInt32 SubID = reader.ReadUInt32();
                    if (SubID == 0xffffffff) break;
                    //UInt32 DataSize = reader.ReadUInt32();
                    long startPos = reader.BaseStream.Position;
                    //Console.WriteLine($"Expected End Position: {(startPos + DataSize):X8}");

                    //UInt32 State = reader.ReadUInt32();
                    //UInt32 LastUpdate = reader.ReadUInt32();
                    //UInt64 AccessToken = reader.ReadUInt64();
                    byte[] Checksum = reader.ReadBytes(20);
                    UInt32 LastChangeNumber = reader.ReadUInt32();

                    BVPropertyCollection Data = BVdfFile.ReadPropertyArray(reader);
                    //long endPos = reader.BaseStream.Position;
                    //if (reader.BaseStream.Position != (startPos + DataSize))
                    //{
                    //    Console.WriteLine("appinfo.vdf chunk data size wrong, adjusting stream position");
                    //    reader.BaseStream.Seek(startPos + DataSize, SeekOrigin.Begin);
                    //}
                    //Console.WriteLine($"*Expected End Position: {(startPos + DataSize):X8}");
                    //Console.WriteLine($"End Position: {(endPos):X8}");

                    Data.ForceObjectWhenUnsure = true;
                    Data.Children()?.ForEach(node =>
                    {
                        BVPropertyCollection appids = ((node.Value as BVPropertyCollection)?["appids"] as BVPropertyCollection);
                        if (appids != null) appids.ForceNumericWhenUnsure = true;

                        BVPropertyCollection depotids = ((node.Value as BVPropertyCollection)?["depotids"] as BVPropertyCollection);
                        if (depotids != null) depotids.ForceNumericWhenUnsure = true;

                        BVPropertyCollection appitems = ((node.Value as BVPropertyCollection)?["appitems"] as BVPropertyCollection);
                        if (appitems != null) appitems.ForceNumericWhenUnsure = true;
                    });

                    SteamPackageInfoDataFileChunk Chunk = new SteamPackageInfoDataFileChunk(SubID, /*State, LastUpdate, AccessToken,*/ Checksum, LastChangeNumber, Data);
                    Chunks.Add(Chunk);
                }
                return new SteamPackageInfoDataFile(Version1, Type, Version2, Version3, Chunks);
            }
        }
    }
}
