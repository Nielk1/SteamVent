using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.SteamClient.Attributes;
using SteamVent.SteamClient.Interop;
using SteamVent.Tools;

namespace SteamVent.SteamClient.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Contains ISteamClient017 (Version ID: 'SteamClient017') delegates which correspond to their native SteamClient DLL functions.
    /// </summary>
    [InterfaceVersion("SteamClient017")]
    public class ISteamClient017 : SteamInterfaceWrapper
    {
        public ISteamClient017(IntPtr interfacePtr) : base(interfacePtr) { }

        #region VTableIndex(0)
        [VTableIndex(0), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 CreateSteamPipeDelegate(IntPtr thisPtr);
        #endregion
        public Int32 CreateSteamPipe() => GetDelegate<CreateSteamPipeDelegate>()(InterfacePtr);

        #region VTableIndex(1)
        [VTableIndex(1), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate bool BReleaseSteamPipeDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        #endregion
        public bool BReleaseSteamPipe(Int32 hSteamPipe) =>
            GetDelegate<BReleaseSteamPipeDelegate>()(InterfacePtr, hSteamPipe);

        #region VTableIndex(2)
        [VTableIndex(2), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Int32 ConnectToGlobalUserDelegate(IntPtr thisPtr, Int32 hSteamPipe);
        #endregion
        public Int32 ConnectToGlobalUser(Int32 hSteamPipe) =>
            GetDelegate<ConnectToGlobalUserDelegate>()(InterfacePtr, hSteamPipe);

        #region VTableIndex(3)
        [VTableIndex(3), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void CreateLocalUserDelegate(IntPtr thisPtr, ref Int32 phSteamPipe, EAccountType eAccountType);
        #endregion
        public void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType) =>
            GetDelegate<CreateLocalUserDelegate>()(InterfacePtr, ref phSteamPipe, eAccountType);

        #region VTableIndex(4)
        [VTableIndex(4), UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void ReleaseUserDelegate(IntPtr thisPtr, Int32 hSteamPipe, Int32 hUser);
        #endregion
        public void ReleaseUser(Int32 hSteamPipe, Int32 hUser) =>
            GetDelegate<ReleaseUserDelegate>()(InterfacePtr, hSteamPipe, hUser);

        #region VTableIndex(5)
        [VTableIndex(5), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetISteamUserDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
        // For pchVersion, use the string "STEAMUSER_INTERFACE_VERSION" to get the current version of the interface
        #endregion
        //public IntPtr GetISteamUser(Int32 hSteamUser, Int32 hSteamPipe, string pchVersion = "STEAMUSER_INTERFACE_VERSION") =>
        //    GetDelegate<GetISteamUserDelegate>()(InterfacePtr, hSteamUser, hSteamPipe, pchVersion.ToUtf8());
        public T GetISteamUser<T>(Int32 hSteamUser, Int32 hSteamPipe) where T : SteamInterfaceWrapper
        {
            IntPtr ptr = GetDelegate<GetISteamUserDelegate>()(InterfacePtr, hSteamUser, hSteamPipe, ((InterfaceVersion)typeof(T).GetCustomAttribute(typeof(InterfaceVersion))).Version.ToUtf8());
            if (ptr == null) return null;
            return (T)Activator.CreateInstance(typeof(T), ptr);
        }

        #region VTableIndex(15)
        [VTableIndex(15), UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetISteamAppsDelegate(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
        // For pchVersion, use the string "STEAMAPPS_INTERFACE_VERSION" to get the current version of the interface
        #endregion
        //public IntPtr GetISteamApps(Int32 hSteamUser, Int32 hSteamPipe, string pchVersion = "STEAMAPPS_INTERFACE_VERSION") =>
        //    GetDelegate<GetISteamAppsDelegate>()(InterfacePtr, hSteamUser, hSteamPipe, pchVersion.ToUtf8());
        public T GetISteamApps<T>(Int32 hSteamUser, Int32 hSteamPipe) where T : SteamInterfaceWrapper
        {
            IntPtr ptr = GetDelegate<GetISteamAppsDelegate>()(InterfacePtr, hSteamUser, hSteamPipe, ((InterfaceVersion)typeof(T).GetCustomAttribute(typeof(InterfaceVersion))).Version.ToUtf8());
            if (ptr == null) return null;
            return (T)Activator.CreateInstance(typeof(T), ptr);
        }
    }
}
