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
using SteamVent.Core.BVdf;
using SteamVent.Steamworks.Native.Interfaces;
using SteamVent.Steamworks.Native;
using Gameloop.Vdf;
using SteamVent.Core;

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

        private readonly object _lifecycleLock = new object();

        /// <summary>
        /// True when the Steamworks interface is currently available for use.
        /// </summary>
        public bool IsAvailable
        {
            get { lock (_lifecycleLock) { return InternalSteamClient != null; } }
        }

        /// <summary>
        /// Ensure Steamworks is initialized (once, thread-safe) when Steam is running, and report
        /// whether the interface is available for use afterwards. Idempotent: it is safe to call on
        /// every operation, and a failed start never wedges subsequent attempts (unlike the old
        /// one-shot gate that returned false after the first successful start).
        /// </summary>
        public bool EnsureSteamworks()
        {
            lock (_lifecycleLock)
            {
                if (InternalSteamClient == null && SteamIsRunning)
                {
                    try
                    {
                        InitSteamworks();
                    }
                    catch
                    {
                        InternalSteamClient = null; // a half-started client must not be reused
                    }
                }
                return InternalSteamClient != null;
            }
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
        /// Shutdown Steamworks if active. Idempotent and thread-safe.
        /// </summary>
        private bool _shutdown;
        private void Shutdown()
        {
            lock (_lifecycleLock)
            {
                if (_shutdown || InternalSteamClient == null)
                    return;
                _shutdown = true;
                try
                {
                    InternalSteamClient.ReleaseUser(Pipe, User);
                    InternalSteamClient.BReleaseSteamPipe(Pipe);
                }
                finally
                {
                    InternalSteamClient = null;
                    Pipe = 0;
                    User = 0;
                }
            }
        }
        #endregion Steamworks Lifecycle

        /// <summary>
        /// The real interop ISteamApps (ISteamApps008); null when Steamworks is unavailable.
        /// This is the actual native interface - distinct from the <see cref="SteamApps"/> wrapper.
        /// </summary>
        public ISteamApps? GetISteamApps()
        {
            if (!EnsureSteamworks() || InternalSteamClient == null)
                return null;
            try
            {
                return InternalSteamClient.GetISteamApps<ISteamApps008>(User, Pipe);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The real interop <see cref="ISteamUtils"/> (ISteamUtils007); null when Steamworks is unavailable.
        /// </summary>
        public ISteamUtils? GetISteamUtils()
        {
            if (!EnsureSteamworks() || InternalSteamClient == null)
                return null;
            try
            {
                return InternalSteamClient.GetISteamUtils<ISteamUtils007>(Pipe);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The real interop <see cref="ISteamUGC"/> (ISteamUGC005); null when Steamworks is unavailable.
        /// </summary>
        public ISteamUGC? GetISteamUGC()
        {
            if (!EnsureSteamworks() || InternalSteamClient == null)
                return null;
            try
            {
                return InternalSteamClient.GetISteamUGC<ISteamUGC005>(User, Pipe);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The live Steam pipe; required by <c>Steam.GetAPICallResult</c> to read a completed async call.
        /// 0 when Steamworks is not (yet) available.
        /// </summary>
        public Int32 SteamPipe => Pipe;







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

        object steamWorkshopLock = new object();
        SteamWorkshop? steamWorkshop = null;
        public SteamWorkshop GetSteamWorkshop()
        {
            lock (steamWorkshopLock)
            {
                steamWorkshop ??= new SteamWorkshop(this);
                return steamWorkshop;
            }
        }
    }
}
