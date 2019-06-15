using System;

namespace SteamVent.InterProc.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InterfaceVersion : Attribute
    {
        public InterfaceVersion(string version) { Version = version; }

        public string Version { get; set; }
    }
}
