using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.Steamworks.Native.Attributes;
using SteamVent.Steamworks.Native;
//using SteamVent.Tools;

namespace SteamVent.Steamworks.Native.Interfaces
{
    /// <summary>
    /// Native warning-message hook callback (see <c>SteamAPIWarningMessageHook_t</c> in the Steamworks SDK).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SteamAPIWarningMessageHook_t(Int32 iSeverity, IntPtr pchWarning);

    public interface ISteamClient
    {
        Int32 CreateSteamPipe();
        bool BReleaseSteamPipe(Int32 hSteamPipe);
        Int32 ConnectToGlobalUser(Int32 hSteamPipe);
        void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType);
        void ReleaseUser(Int32 hSteamPipe, Int32 hUser);
        TInterface GetISteamUser<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        TInterface GetISteamUtils<TInterface>(Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        TInterface GetISteamApps<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        TInterface GetISteamUGC<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        void SetLocalIPBinding(UInt32 unIP, UInt16 usPort);
        void RunFrame();
        void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction);
        bool BShutdownIfAllPipesClosed();
    }
}
