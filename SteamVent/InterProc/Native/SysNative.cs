using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SteamVent.InterProc.Native
{
    public struct SysNative
    {
        public const UInt32 LOAD_WITH_ALTERED_SEARCH_PATH = 8;
        public const int WM_GETTEXT = 0x0D;
        public const int WM_GETTEXTLENGTH = 0x0E;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpszLib, IntPtr hFile, UInt32 dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr SetDllDirectory(string lpPathName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int param, System.Text.StringBuilder text);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);



        /// <summary>
        /// Defines whether the application is running in 64-bit mode.
        /// </summary>
        /// <returns>true if the app is running in 64-bit mode; otherwise, false.</returns>
        public static bool Is64Bit() { return IntPtr.Size == 8; }

        /// <summary>
        /// Defines weather the application is running on a 64bit OS
        /// </summary>
        /// <returns>true if the app is 32bit running on 64bit</returns>
        public static bool IsSystem64Bit() {return Is64Bit() || IsWow64(); }

        /// <summary>
        /// Defines weather the application is running 32-bit via Wow64
        /// </summary>
        /// <returns>true if the app is 32-bit running on 64-bit OS</returns>
        public static bool IsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the specified function from an unmanaged module's export address table and assigns it to the designated delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type that represents the export function.</typeparam>
        /// <param name="module">A pointer to the unmanaged module to lookup the export function in.</param>
        /// <param name="name">A string defining the name of the export function to retrieve.</param>
        /// <returns>A reference to the unmanaged export function of the delegate type provided; returns null upon failure.</returns>
        public static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name) where TDelegate : class
        {
            IntPtr address = GetProcAddress(module, name);

            if (address == IntPtr.Zero)
                return null;

            return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        //public static string PtrToStringUtf8(IntPtr stringPtr)
        //{
        //    var byteList = new List<byte>();
        //    int i = 0;
        //    do
        //    {
        //        byteList.Add(Marshal.ReadByte(stringPtr, i));
        //        i++;
        //    } while (byteList[i - 1] != 0x0);

        //    return Encoding.UTF8.GetString(byteList.ToArray());
        //}
    }
}
