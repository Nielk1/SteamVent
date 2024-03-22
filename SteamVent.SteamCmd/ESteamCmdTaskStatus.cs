using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public enum ESteamCmdTaskStatus
    {
        WaitingToStart,
        Running,
        Waiting,
        Finished,
    }
}