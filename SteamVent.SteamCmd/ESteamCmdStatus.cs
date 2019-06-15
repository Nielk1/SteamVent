using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public enum ESteamCmdStatus
    {
        Unknown,
        Extracting,
        Downloading,
        Installed,
        Starting,
        Active,
        //Exiting,
        Closed,
        //LoggedIn,
        //LoggedInAnon,
    }
}
