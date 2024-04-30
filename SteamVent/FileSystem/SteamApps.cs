using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.FileSystem
{
    public class SteamApps
    {
        public static string? GetAppInstallDir(UInt32 appID)
        {
            foreach (string basePath in SteamProcessInfo.GetSteamLibraryPaths())
            {
                string manifestPath = Path.Combine(basePath, "steamapps", $"appmanifest_{appID}.acf");
                if (File.Exists(manifestPath))
                {
                    VProperty data = VdfConvert.Deserialize(File.ReadAllText(manifestPath));
                    string? installdir = data.Value.Value<string>("installdir");
                    if (!string.IsNullOrWhiteSpace(installdir))
                    {
                        string path = Path.Combine(basePath, "steamapps", "common", installdir);
                        if (Directory.Exists(path))
                            return path;
                    }
                }
            }
            return null;
        }
    }
}
