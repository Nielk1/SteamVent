using SteamVent.InterProc;
using SteamVent.InterProc.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SteamVent.Adaptive
{
    public class SteamContext : IDisposable
    {
        #region Dispose
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Shutdown();
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        ~SteamContext()
        {
            Dispose(false);
        }
        #endregion Dispose

        Steamworks.SteamClient steamworksClient;

        public SteamContext()
        {
            steamworksClient = Steamworks.SteamClient.Construct();
        }
        private void Shutdown()
        {
            steamworksClient.Dispose();
        }


        object steamAppsLock = new object();
        SteamApps? steamApps = null;
        public SteamApps GetSteamApps()
        {
            lock (steamAppsLock)
            {
                steamApps ??= new SteamApps(this, steamworksClient);
                return steamApps;
            }
        }


        object steamWorkshopLock = new object();
        SteamWorkshop? steamWorkshop = null;
        public SteamWorkshop GetSteamWorkshop()
        {
            lock (steamWorkshopLock)
            {
                steamWorkshop ??= new SteamWorkshop(this, steamworksClient);
                return steamWorkshop;
            }
        }
    }
}
