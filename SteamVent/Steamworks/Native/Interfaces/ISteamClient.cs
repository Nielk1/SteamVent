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
        static readonly public Type[] Versions =
        {
            typeof(ISteamClient007),
            typeof(ISteamClient008),
            typeof(ISteamClient009),
            typeof(ISteamClient010),
            typeof(ISteamClient011),
            typeof(ISteamClient012),
            typeof(ISteamClient013),
            typeof(ISteamClient014),
            typeof(ISteamClient015),
            typeof(ISteamClient016),
            typeof(ISteamClient017),
            typeof(ISteamClient018),
            typeof(ISteamClient019),
        };

        Int32 CreateSteamPipe();
        FunctionImplemented CreateSteamPipeSupport { get; }

        bool BReleaseSteamPipe(Int32 hSteamPipe);
        FunctionImplemented BReleaseSteamPipeSupport { get; }

        Int32 ConnectToGlobalUser(Int32 hSteamPipe);
        FunctionImplemented ConnectToGlobalUserSupport { get; }

        void CreateLocalUserLegacy(ref Int32 phSteamPipe);
        FunctionImplemented CreateLocalUserLegacySupport { get; }
        void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType);
        FunctionImplemented CreateLocalUserSupport { get; }

        void ReleaseUser(Int32 hSteamPipe, Int32 hUser);
        FunctionImplemented ReleaseUserSupport { get; }

        TInterface GetISteamUser<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        FunctionImplemented GetISteamUserSupport { get; }

        TInterface GetISteamUtils<TInterface>(Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        FunctionImplemented GetISteamUtilsSupport { get; }

        TInterface GetISteamApps<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        FunctionImplemented GetISteamAppsSupport { get; }

        TInterface GetISteamUGC<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        FunctionImplemented GetISteamUGCSupport { get; }

        void SetLocalIPBinding(UInt32 unIP, UInt16 usPort);
        FunctionImplemented SetLocalIPBindingSupport { get; }

        void RunFrame();
        FunctionImplemented RunFrameSupport { get; }

        void SetWarningMessageHook(SteamAPIWarningMessageHook_t pFunction);
        FunctionImplemented SetWarningMessageHookSupport { get; }

        bool BShutdownIfAllPipesClosed();
        FunctionImplemented BShutdownIfAllPipesClosedSupport { get; }
    }
}
