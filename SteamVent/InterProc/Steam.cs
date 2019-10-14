using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SteamVent.FileSystem;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interfaces;
using SteamVent.InterProc.Interop;
using SteamVent.InterProc.Native;

namespace SteamVent.InterProc
{
    public class Steam
    {
        private static IntPtr SteamClientHandle = IntPtr.Zero;

        private static IntPtr SteamHandle = IntPtr.Zero;

        /// <summary>
        /// Loads the steamclient library. This does not load the steam library. Please use the overload to do so.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool Load()
        {
            return LoadSteamClient();
        }

        /// <summary>
        /// Loads the steam library.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        /*public static bool LoadSteam()
        {
            if (SteamHandle != IntPtr.Zero)
                return true;

            string path = SteamProcessInfo.SteamInstallPath;

            if (!string.IsNullOrEmpty(path))
                SysNative.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));

            path = Path.Combine(path, "steam.dll");

            IntPtr module = SysNative.LoadLibraryEx(path, IntPtr.Zero, SysNative.LOAD_WITH_ALTERED_SEARCH_PATH);
            
            if (module == IntPtr.Zero)
                return false;

            _callCreateSteamInterface = SysNative.GetExportFunction<SteamNative._f>(module, "_f");
            if (_callCreateSteamInterface == null)
                return false;

            SteamHandle = module;

            return true;
        }*/

        /// <summary>
        /// Loads the steamclient library.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool LoadSteamClient()
        {
            if (SteamClientHandle != IntPtr.Zero)
                return true;

            string path = SteamProcessInfo.SteamInstallPath;

            if (!string.IsNullOrEmpty(path))
                SysNative.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));

            if (SysNative.Is64Bit())
                path = Path.Combine(path, "steamclient64.dll");
            else
                path = Path.Combine(path, "steamclient.dll");

            IntPtr module = SysNative.LoadLibraryEx(path, IntPtr.Zero, SysNative.LOAD_WITH_ALTERED_SEARCH_PATH);

            if (module == IntPtr.Zero)
                return false;

            _callCreateInterface = SysNative.GetExportFunction<SteamNative.CreateInterface>(module, "CreateInterface");
            if (_callCreateInterface == null)
                return false;

            _callSteamBGetCallback = SysNative.GetExportFunction<SteamNative.SteamBGetCallback>(module, "Steam_BGetCallback");
            if (_callSteamBGetCallback == null)
                return false;

            _callSteamFreeLastCallback = SysNative.GetExportFunction<SteamNative.SteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_callSteamFreeLastCallback == null)
                return false;

            CallSteamGetAPICallResult = SysNative.GetExportFunction<SteamNative.SteamGetAPICallResult>(module, "Steam_GetAPICallResult");
            if (CallSteamGetAPICallResult == null)
                return false;

            SteamClientHandle = module;

            return true;
        }

        #region Delegate Implementations

        /// <summary>
        /// An instance of the <see cref="SteamNative.CreateInterface"/> delegate.
        /// </summary>
        private static SteamNative.CreateInterface _callCreateInterface;

        /// <summary>
        /// Gets an unmanaged handle to an instance of the specified SteamClient interface.
        /// </summary>
        /// <param name="version">A string defining the desired interface and version.</param>
        /// <param name="returnCode">An IntPtr value to return if the call fails.</param>
        /// <returns>A handle to the unmanaged Steam Client interface, or the provided <paramref name="returnCode"/> value upon failure.</returns>
        public static TInterface CreateInterface<TInterface>() where TInterface : SteamInterfaceWrapper
        {
            if (_callCreateInterface == null)
                throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(CreateInterface)}).");

            string version = ((InterfaceVersion)typeof(ISteamClient017).GetCustomAttribute(typeof(InterfaceVersion))).Version;

            IntPtr ptr = _callCreateInterface(version, IntPtr.Zero);
            if (ptr == null) return null;
            if (ptr == IntPtr.Zero) return null;
            return (TInterface)Activator.CreateInstance(typeof(TInterface), ptr);
        }

        /// <summary>
        /// An instance of the <see cref="SteamNative._f"/> delegate.
        /// </summary>
        private static SteamNative._f _callCreateSteamInterface;

        /// <summary>
        /// Gets an unmanaged handle to an instance of the specified Steam interface.
        /// </summary>
        /// <param name="version">A string defining the desired interface and version.</param>
        /// <returns>A handle to the unmanaged Steam interface, or null upon failure.</returns>
        public static IntPtr CreateSteamInterface(string version)
        {
            if (_callCreateSteamInterface == null)
                throw new InvalidOperationException($"Steam library is not initialized ({nameof(CreateSteamInterface)}).");

            return _callCreateSteamInterface(version);
        }

        private static SteamNative.SteamBGetCallback _callSteamBGetCallback;
        /// <summary>
        /// Gets the last callback in steamclient's callback queue.
        /// </summary>
        /// <param name="pipe">The steam pipe.</param>
        /// <param name="message">A reference to a callback object to copy the callback to.</param>
        /// <returns>True if a callback was copied, or false if no callback was waiting, or an error occured.</returns>
        public static bool GetCallback(int pipe, ref CallbackMsg_t message)
        {
            if (_callSteamBGetCallback == null)
                throw new InvalidOperationException($"Steam library is not been initialized({nameof(GetCallback)}).");

            try
            {
                return _callSteamBGetCallback(pipe, ref message);
            }
            catch
            {
                message = new CallbackMsg_t();
                return false;
            }
        }

        private static SteamNative.SteamFreeLastCallback _callSteamFreeLastCallback;
        /// <summary>
        /// Frees the last callback in steamclient's callback queue.
        /// </summary>
        /// <param name="pipe">The steam pipe.</param>
        /// <returns>True if the callback was freed; otherwise, false.</returns>
        public static bool FreeLastCallback(int pipe)
        {
            if (_callSteamFreeLastCallback == null)
                throw new InvalidOperationException($"Steam library has not been initialized({nameof(FreeLastCallback)}).");

            return _callSteamFreeLastCallback(pipe);
        }

        private static SteamNative.SteamGetAPICallResult CallSteamGetAPICallResult;

        public static bool GetAPICallResult(int hSteamPipe, ulong hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, ref bool pbFailed)
        {
            if (CallSteamGetAPICallResult == null)
                throw new InvalidOperationException($"Steam library has not been initialized({nameof(GetAPICallResult)}).");

            return CallSteamGetAPICallResult(hSteamPipe, hSteamAPICall, pCallback, cubCallback, iCallbackExpected, ref pbFailed);
        }

        ///// <summary>
        ///// An instance of the <see cref="SteamNative.CreateSteamPipe"/> delegate.
        ///// </summary>
        //private static SteamNative.CreateSteamPipe _callCreateSteamPipe;

        ///// <summary>
        ///// Creates a communication pipe with Steam to facilitate execution of additional Client functions.
        ///// </summary>
        ///// <returns>An int representing a specific Steam communication pipe.</returns>
        //private static int CreateSteamPipe()
        //{
        //    if (_callCreateSteamPipe == null)
        //        throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(CreateSteamPipe)}).");

        //    return _callCreateSteamPipe();
        //}

        ///// <summary>
        ///// An instance of the <see cref="SteamNative.BReleaseSteamPipe"/> delegate.
        ///// </summary>
        //private static SteamNative.BReleaseSteamPipe _callBReleaseSteamPipe;

        ///// <summary>
        ///// Releases the previously acquired communication pipe to the Steam Client.
        ///// </summary>
        ///// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        ///// <returns>true if the provided pipe value was valid and released successfully; otherwise, false.</returns>
        //private static bool BReleaseSteamPipe(int pipe)
        //{
        //    if (_callBReleaseSteamPipe == null)
        //        throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(BReleaseSteamPipe)}).");

        //    return _callBReleaseSteamPipe(pipe);
        //}

        ///// <summary>
        ///// An instance of the <see cref="SteamNative.ConnectToGlobalUser"/> delegate.
        ///// </summary>
        //private static SteamNative.ConnectToGlobalUser _callConnectToGlobalUser;

        ///// <summary>
        ///// Connects to an existing global Steam user.
        ///// </summary>
        ///// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        ///// <returns>An int defining a connection to an existing global Steam user; returns 0 if call failed for any reason.</returns>
        //private static int ConnectToGlobalUser(int pipe)
        //{
        //    if (_callConnectToGlobalUser == null)
        //        throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(ConnectToGlobalUser)}).");

        //    return _callConnectToGlobalUser(pipe);
        //}

        ///// <summary>
        ///// An instance of the <see cref="SteamNative.ReleaseUser"/> delegate.
        ///// </summary>
        //private static SteamNative.ReleaseUser _callReleaseUser;

        ///// <summary>
        ///// Releases the ongoing connection to a global Steam user; this should be called prior to terminating a communication pipe.
        ///// </summary>
        ///// <param name="pipe">The Steam communication pipe acquired by calling <see cref="CreateSteamPipe"/>.</param>
        ///// <param name="user">An int defining a connection to an existing global Steam user.</param>
        //private static void ReleaseUser(int pipe, int user)
        //{
        //    if (_callReleaseUser == null)
        //        throw new InvalidOperationException($"Steam Client library is not initialized ({nameof(ReleaseUser)}).");

        //    _callReleaseUser(pipe, user);
        //}

        #endregion
    }
}
