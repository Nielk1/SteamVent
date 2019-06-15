using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    class SteamCmdException : Exception
    {
        public SteamCmdException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    class SteamCmdMissingException : SteamCmdException
    {
        public SteamCmdMissingException(string msg)
            : base(msg)
        {
        }
    }

    class SteamCmdWorkshopDownloadException : SteamCmdException
    {
        public SteamCmdWorkshopDownloadException(string msg)
            : base(msg)
        {
        }
    }
}
