using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Gameloop.Vdf.Linq;
using SteamVent.FileSystem;
using SteamVent.Common.BVdf;
using SteamVent.InterProc.Interfaces;
using SteamVent.InterProc;
using Gameloop.Vdf;
using SteamVent.Common;

namespace SteamVent.Steamworks
{
    public class SteamClient : IDisposable
    {
        private ISteamClient? InternalSteamClient { get; set; }
        private Int32 Pipe { get; set; }
        private Int32 User { get; set; }

        private static object instanceLock = new object();
        private static SteamClient? instance = null;
        private static int instanceCounter = 0;

        /// <summary>
        /// Get the existing instance without incrementing permits.
        /// If you're using this consider redesign to use an instance variable from the Construct call.
        /// As this is a singleton the Construct and Dispose functions use permit counting to only close the interface when no users remain.
        /// </summary>
        public SteamClient Instance => instance ?? throw new Exception("SteamClient instance not created");

        private SteamClient()
        {
            //InitSteamworks();
        }

        /// <summary>
        /// Construct a new SteamClient instance, must be disposed when done
        /// </summary>
        /// <returns></returns>
        public static SteamClient Construct()
        {
            lock (instanceLock)
            {
                if (instance == null)
                    instance = new SteamClient();
                instanceCounter++;
                return instance;
            }
        }

        #region Dispose
        bool disposed = false;

        public void Dispose()
        {
            bool doDispose = false;
            lock (instanceLock)
            {
                instanceCounter--;
                if (instanceCounter == 0)
                {
                    instance = null;
                    doDispose = true;
                }
            }
            if (doDispose)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Shutdown();
            }

            disposed = true;
        }

        ~SteamClient()
        {
            Dispose(false);
        }
        #endregion Dispose

        #region Steamworks Lifecycle
        public bool SteamIsRunning { get { return SteamProcessInfo.GetSteamPid() > 0; } }

        /// <summary>
        /// Inilize Steamworks if not already active
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal bool TryStartSteamworks()
        {
            if (SteamIsRunning && InternalSteamClient == null)
            {
                InitSteamworks();
                return InternalSteamClient != null;
            }
            return false;
        }
        
        /// <summary>
        /// Inilize Steamworks
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void InitSteamworks()
        {
            Steam.Load();
            InternalSteamClient = Steam.CreateInterface<ISteamClient017>();
            if (InternalSteamClient == null)
                throw new Exception();
            Pipe = InternalSteamClient.CreateSteamPipe();
            if (Pipe == 0)
                throw new Exception();
            User = InternalSteamClient.ConnectToGlobalUser(Pipe);
            if (User == 0)
                throw new Exception();
            //SteamApps = InternalSteamClient.GetISteamApps<ISteamApps008>(User, Pipe);
            //if (SteamApps == null)
            //    throw new Exception();
        }
        
        /// <summary>
        /// Shutdown Steamworks if active
        /// </summary>
        private void Shutdown()
        {
            if (InternalSteamClient == null)
                return;

            InternalSteamClient.ReleaseUser(Pipe, User);
            InternalSteamClient.BReleaseSteamPipe(Pipe);
        }
        #endregion Steamworks Lifecycle







        object steamAppsLock = new object();
        SteamApps? steamApps = null;
        public SteamApps GetSteamApps()
        {
            lock (steamAppsLock)
            {
                steamApps ??= new SteamApps(this);
                return steamApps;
            }
        }
    }
}
