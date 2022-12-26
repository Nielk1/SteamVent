using SteamVent.Common.BVdf;
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

        public static SteamAppInfoDataFile GetSteamAppInfoDataFile(string path = null)
        {
            return SteamAppInfoDataFile.Read(path ?? GetAppCacheAppInfoFile());
        }





        public byte Version1;
        public UInt16 Type; // 0x4456 ('DV')
        public byte Version2;
        public UInt32 Version3;
        public List<SteamAppInfoDataFileChunk> chunks;

        public SteamAppInfoDataFile(byte Version1, UInt16 Type, byte Version2, UInt32 Version3, List<SteamAppInfoDataFileChunk> chunks)
        {
            this.Version1 = Version1;
            this.Type = Type;
            this.Version2 = Version2;
            this.Version3 = Version3;
            this.chunks = chunks;
        }

        public class SteamAppInfoDataFileChunk
        {
            public UInt32 AppID;
            public UInt32 State;
            public UInt32 LastUpdate;
            public UInt64 AccessToken;
            public byte[] Checksum;
            public UInt32 LastChangeNumber;
            public BVPropertyCollection data;

            public SteamAppInfoDataFileChunk(UInt32 AppID, UInt32 State, UInt32 LastUpdate, UInt64 AccessToken, byte[] Checksum, UInt32 LastChangeNumber, BVPropertyCollection data)
            {
                this.AppID = AppID;
                this.State = State;
                this.LastUpdate = LastUpdate;
                this.AccessToken = AccessToken;
                this.Checksum = Checksum;
                this.LastChangeNumber = LastChangeNumber;
                this.data = data;
            }
        }

        public static SteamAppInfoDataFile Read(string steamShortcutFilePath)
        {
            using (FileStream stream = File.OpenRead(steamShortcutFilePath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                List<SteamAppInfoDataFileChunk> Chunks = new List<SteamAppInfoDataFileChunk>();
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

                    BVPropertyCollection Data;
                    if (Version1 <= 0x26)
                    {
                        BVPropertyCollection AppInfoWrapperNodeData = new BVPropertyCollection();
                        AppInfoWrapperNodeData.Add(new BVProperty("appid", AppID));

                        byte ReadByte = 0x00;
                        while ((ReadByte = reader.ReadByte()) != 0x08)
                        {
                             if (ReadByte == 0x00)
                                break; // while 0x08 means end of section, 0x00 means no section started at all
                            ReadByte = reader.ReadByte(); // chew off the 0x00 for the property set we started, TODO: move this into
                            AppInfoWrapperNodeData.Add(BVdfFile.ReadProperty(ReadByte, reader));
                            reader.ReadByte(); // chew off the 0x08 for the section set we started
                        }

                        BVProperty AppInfoWrapperNode = new BVProperty("appinfo", AppInfoWrapperNodeData);
                        Data = new BVPropertyCollection();
                        Data.Add(AppInfoWrapperNode);
                    }
                    else
                    {
                        Data = BVdfFile.ReadPropertyArray(reader);
                    }
                    //long endPos = reader.BaseStream.Position;
                    if (reader.BaseStream.Position != (startPos + DataSize))
                    {
                        Console.WriteLine("appinfo.vdf chunk data size wrong, adjusting stream position");
                        reader.BaseStream.Seek(startPos + DataSize, SeekOrigin.Begin);
                    }
                    //Console.WriteLine($"*Expected End Position: {(startPos + DataSize):X8}");
                    //Console.WriteLine($"End Position: {(endPos):X8}");

                    SteamAppInfoDataFileChunk Chunk = new SteamAppInfoDataFileChunk(AppID, State, LastUpdate, AccessToken, Checksum, LastChangeNumber, Data);
                    Chunks.Add(Chunk);
                }
                return new SteamAppInfoDataFile(Version1, Type, Version2, Version3, Chunks);
            }
        }
    }
}
