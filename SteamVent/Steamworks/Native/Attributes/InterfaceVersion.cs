using System;

namespace SteamVent.Steamworks.Native.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InterfaceVersion : Attribute
    {
        public InterfaceVersion(string version) { Version = version; }

        public string Version { get; set; }
    }
}
