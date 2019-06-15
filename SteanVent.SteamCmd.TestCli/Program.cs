using SteamVent.SteamCmd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteanVent.SteamCmd.TestCli
{
    class Program
    {
        static object OutLock = new object();
        static void Main(string[] args)
        {
            WriteLine("Start");
            SteamCmdContext steamcmd = SteamCmdContext.GetInstance();
            steamcmd.SteamCmdStatusChange += Steamcmd_SteamCmdStatusChange;
            WriteLine(new string('-', Console.WindowWidth - 1));

            WriteLine("Download if needed");
            steamcmd.Download();
            WriteLine(new string('-', Console.WindowWidth - 1));

            WriteLine("Status Check");
            List<WorkshopItemStatus> status = steamcmd.WorkshopStatus(624970);
            WriteLine(status.Count);
            WriteLine("WorkshopId\tStatus   \tHasUpdate\tSize\tDateTime");
            foreach (var stat in status)
            {
                WriteLine($"{stat.WorkshopId}\t{stat.Status}\t{stat.HasUpdate}    \t{stat.Size}\t{stat.DateTime}");
            }
            WriteLine(new string('-', Console.WindowWidth - 1));

            WriteLine("Mod Download");
            string downloadString = steamcmd.WorkshopDownloadItem(624970, 1300825258);
            WriteLine(downloadString);
            WriteLine(new string('-', Console.WindowWidth - 1));

            WriteLine("Status Check");
            status = steamcmd.WorkshopStatus(624970);
            WriteLine(status.Count);
            WriteLine("WorkshopId\tStatus   \tHasUpdate\tSize\tDateTime");
            foreach (var stat in status)
            {
                WriteLine($"{stat.WorkshopId}\t{stat.Status}\t{stat.HasUpdate}    \t{stat.Size}\t{stat.DateTime}");
            }
            WriteLine(new string('-', Console.WindowWidth - 1));

            WriteLine("End, Press any key to exit.");
            Console.ReadKey(true);
        }

        private static void Steamcmd_SteamCmdStatusChange(object sender, SteamCmdStatusChangeEventArgs e)
        {
            WriteLine("SteamCmd Status: {0}", e.Status);
        }




        private static void WriteLine()
        {
            lock (OutLock)
            {
                Console.WriteLine();
            }
        }
        private static void WriteLine(object message)
        {
            lock (OutLock)
            {
                Console.WriteLine(message?.ToString());
            }
        }
        private static void WriteLine(string message, params object[] args)
        {
            lock(OutLock)
            {
                Console.WriteLine(message, args);
            }
        }
    }
}
