using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SteamVent.InterProc.Attributes;
using SteamVent.InterProc.Interop;
using SteamVent.Tools;

namespace SteamVent.InterProc.Interfaces
{
    public interface ISteamClient
    {
        Int32 CreateSteamPipe();
        bool BReleaseSteamPipe(Int32 hSteamPipe);
        Int32 ConnectToGlobalUser(Int32 hSteamPipe);
        void CreateLocalUser(ref Int32 phSteamPipe, EAccountType eAccountType);
        void ReleaseUser(Int32 hSteamPipe, Int32 hUser);
        TInterface GetISteamUser<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
        TInterface GetISteamApps<TInterface>(Int32 hSteamUser, Int32 hSteamPipe) where TInterface : SteamInterfaceWrapper;
    }
}
