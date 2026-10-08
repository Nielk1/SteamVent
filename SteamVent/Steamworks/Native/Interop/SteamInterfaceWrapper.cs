using SteamVent.Steamworks.Native.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SteamVent.Steamworks.Native
{
    /// <summary>
    /// Generic wrapper class for any managed SteamClient DLL native interface implementation.
    /// </summary>
    public abstract class SteamInterfaceWrapper
    {
        protected SteamInterfaceWrapper(IntPtr interfacePtr)
        {
            if (interfacePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument must contain a valid non-zero pointer value.");

            InterfacePtr = interfacePtr;
            VTablePtr = Marshal.ReadIntPtr(InterfacePtr);

            if (VTablePtr == IntPtr.Zero)
                throw new ArgumentException($"The '{nameof(interfacePtr)}' argument does not point to a valid virtual function table.");
        }

        protected IntPtr InterfacePtr { get; }

        protected IntPtr VTablePtr { get; }

        protected Dictionary<Type, Delegate> DelegateCache { get; } = new Dictionary<Type, Delegate>();

        protected TInterface GetInterface<TDelegate, TInterface>(params object[] parameters)
            where TDelegate : Delegate
            where TInterface : SteamInterfaceWrapper
        {
            string version = ((InterfaceVersion)typeof(TInterface).GetCustomAttribute(typeof(InterfaceVersion))).Version;

            IntPtr versionUtf8 = Marshal.StringToCoTaskMemUTF8(version);
            IntPtr ptr;
            try
            {
                ptr = (IntPtr)GetDelegate<TDelegate>()
                    .DynamicInvoke(parameters.Concat(new object[] { versionUtf8 }).ToArray());
            }
            finally
            {
                Marshal.ZeroFreeCoTaskMemUTF8(versionUtf8);
            }

            if (ptr == IntPtr.Zero)
                return null;

            return (TInterface)Activator.CreateInstance(typeof(TInterface), ptr);
        }

        protected TDelegate GetDelegate<TDelegate>()
            where TDelegate : Delegate
        {
            lock (DelegateCache)
            {
                if (!DelegateCache.TryGetValue(typeof(TDelegate), out Delegate func))
                {
                    func = GetFunction(typeof(TDelegate));
                    if (func != null)
                        DelegateCache.Add(typeof(TDelegate), func);
                }

                return func as TDelegate; // returns null if type conversion fails
            }
        }

        /// <summary>
        /// Uses the vtable index tied to the provided delegate type to calculate its unmanaged function pointer and wire it up to its associated 'wrapper' method.
        /// </summary>
        /// <param name="d">The delegate type to lookup</param>
        /// <returns>A delegate that now references its associated native function</returns>
        protected Delegate GetFunction(Type d)
        {
            int vtableIndex = ((VTableIndex)(d.GetCustomAttribute(typeof(VTableIndex)))).Index;
            IntPtr functionPtr = Marshal.ReadIntPtr(VTablePtr, vtableIndex * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer(functionPtr, d);
        }

        // Not currently being used
        //protected TDelegate GetFunction<TDelegate>(TDelegate d)
        //{
        //    int vtableIndex = ((VTableIndex)(typeof(TDelegate).GetCustomAttribute(typeof(VTableIndex)))).Index;
        //    IntPtr functionPtr = Marshal.ReadIntPtr(VTablePtr, vtableIndex * IntPtr.Size);
        //    return Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPtr);
        //}

        /// <summary>
        /// Converts a UTF-8 string stored at the provided pointer into a normal string object. This is needed because Steam stores strings in UTF-8.
        /// </summary>
        /// <param name="stringPtr">A pointer referencing a null-terminated UTF-8 string (or null/zero for an empty result).</param>
        /// <returns>A properly decoded string object, or null when <paramref name="stringPtr"/> is null or zero.</returns>
        protected static string DecodeUtf8String(IntPtr stringPtr) =>
            Marshal.PtrToStringUTF8(stringPtr);
    }
}
