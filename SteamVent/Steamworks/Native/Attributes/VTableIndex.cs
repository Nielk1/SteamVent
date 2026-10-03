using System;

namespace SteamVent.Steamworks.Native.Attributes
{
    [AttributeUsage(AttributeTargets.Delegate, AllowMultiple = false)]
    public class VTableIndex : Attribute
    {
        public VTableIndex(int i) { Index = i; }

        public int Index { get; set; }
    }
}
