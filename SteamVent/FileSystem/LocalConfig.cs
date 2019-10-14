using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.FileSystem
{
    public class LocalConfig
    {
        /// <summary>
        /// User Local Config File
        /// </summary>
        //public static string GetUserLocalConfigFile()
        //{
        //    uint userid = SteamProcessInfo.CurrentUserID;

        //    string installPath = SteamProcessInfo.SteamInstallPath;

        //    if (userid > 0)
        //    {
        //        string shortcutFile = Path.Combine(installPath, @"userdata", userid.ToString(), @"config", @"localconfig.vdf");
        //        if (File.Exists(shortcutFile))
        //            return shortcutFile;
        //    }
        //    /*else
        //    {
        //        string basePath = Path.Combine(installPath, @"userdata");
        //        string path = Directory.GetDirectories(basePath).OrderByDescending(dr => new DirectoryInfo(dr).LastAccessTimeUtc).FirstOrDefault();
        //        if (path != null)
        //        {
        //            string shortcutFile = Path.Combine(path, @"config", @"localconfig.vdf");
        //            if (File.Exists(shortcutFile))
        //                return shortcutFile;
        //        }
        //    }*/

        //    return null;
        //}

        /*public static uint[] GetClientAppIds()
        {
            uint[] localAppIDsAppIDs = null;
            string localconfig = GetUserLocalConfigFile();
            {
                VObject obj = (VObject)VdfConvert.Deserialize(File.ReadAllText(localconfig), new VdfSerializerSettings() { UsesEscapeSequences = true }).Value;
                VObject UserLocalConfigStore = (VObject)obj["UserLocalConfigStore"];
                VObject appTickets = (VObject)UserLocalConfigStore["apptickets"];
                localAppIDsAppIDs = appTickets
                    .Children()
                    .Select(dr =>
                    {
                        uint tmp = 0;
                        if (uint.TryParse(dr.Key, out tmp))
                        {
                            return (uint?)tmp;
                        }
                        return null;
                    }).Where(dr => dr != null).Select(dr => dr.Value).ToArray();
            }

            uint[] installedAppIDs = null;
            //if (ClientAppManager != null)
            //{
            //    uint InstalledAppIDCount = ClientAppManager.GetNumInstalledApps();
            //    installedAppIDs = new uint[InstalledAppIDCount];
            //    ClientAppManager.GetInstalledApps(ref installedAppIDs[0], InstalledAppIDCount);
            //}
            //else
            {
                installedAppIDs = new uint[0];
            }

            return localAppIDsAppIDs.Union(installedAppIDs).OrderBy(dr => dr).ToArray();
        }*/
    }
}
