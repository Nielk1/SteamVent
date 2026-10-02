using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.SteamCmd
{
    public class SteamCmdException : Exception
    {
        public SteamCmdException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class SteamCmdMissingException : SteamCmdException
    {
        public SteamCmdMissingException(string msg)
            : base(msg)
        {
        }
    }

    /// <summary>
    /// Raised when SteamCmd itself could not be obtained -- the download of steamcmd.zip or the
    /// extraction of steamcmd.exe failed (network error, non-2xx response, bad/locked archive,
    /// etc.). Distinct from <see cref="SteamCmdMissingException"/>, which is a runtime symptom of
    /// exactly this: the binary is absent because the setup step never succeeded.
    /// </summary>
    public class SteamCmdDownloadException : SteamCmdException
    {
        public SteamCmdDownloadException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }

    public class SteamCmdWorkshopDownloadException : SteamCmdException
    {
        public SteamCmdWorkshopDownloadException(string msg)
            : base(msg)
        {
        }
    }
}
