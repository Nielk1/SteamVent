using System;
using System.Runtime.InteropServices;
using SteamVent.InterProc.Interfaces;

namespace SteamVent.InterProc.Native
{
    public struct SteamNative
    {
        // Creates an interface from Steam
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr _f(string version);

        // Creates an interface from Steam Client
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate IntPtr CreateInterface(string version, IntPtr returnCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool SteamBGetCallback(int pipe, ref CallbackMsg_t message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool SteamGetAPICallResult(int hSteamPipe, ulong hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, ref bool pbFailed);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool SteamFreeLastCallback(int pipe);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate int CreateSteamPipe();

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.I1)]
        //public delegate bool BReleaseSteamPipe(int pipe);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate int ConnectToGlobalUser(int pipe);

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void ReleaseUser(int pipe, int user);

        //[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        //public delegate IntPtr GetIClientShortcuts(IntPtr thisptr, int user, int pipe, string version);

        //[UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        //public delegate IntPtr GetISteamClient017(IntPtr thisptr, int user, int pipe, string version);
    }
}
