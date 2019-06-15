using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class SteamCmdStatusChangeEventArgs : EventArgs
    {
        public ESteamCmdStatus Status = ESteamCmdStatus.Unknown;

        public SteamCmdStatusChangeEventArgs(ESteamCmdStatus Status) : base()
        {
            this.Status = Status;
        }
    }
}
