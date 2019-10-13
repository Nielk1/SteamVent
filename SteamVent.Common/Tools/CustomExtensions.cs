using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Management;
using System.Text;

namespace SteamVent.Common.Tools
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Hackish workaround to get around the fact that the 'UnmanagedFunctionPointer' CharSet decorator does not support CharSet.UTF8.
        /// This function converts a normal string into a UTF-8 byte array which is then turned back into a normal string using 'Default' 
        /// encoding (ANSI). The end result of this is that when the CLR marshals the string, decoding it into a byte array using ANSI, to 
        /// pass it to a native function, it will actually be a valid UTF-8 string that Steam will read correctly rather than an ANSI string 
        /// which will mangle any non-ASCII special characters. Note: Once converted with this method, the .NET string object will appear 
        /// mangled because its being displayed using the wrong encoding.
        /// </summary>
        /// <param name="str">The string to convert to UTF-8.</param>
        /// <returns>A string that when decoded into bytes using ANSI encoding, can be turned back into a valid string using UTF-8 encoding.</returns>
        public static string ToUtf8(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return Encoding.Default.GetString(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Inserts double quotes around the provided string.
        /// </summary>
        /// <param name="str">The string to put inside double quotation marks.</param>
        /// <returns>Double quoted string if input is not null and length is greater than 0; otherwise, returns an empty string.</returns>
        public static string InDblQuotes(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            if (str.StartsWith("\"") && str.EndsWith("\""))
                return str;

            return $"\"{str}\"";
        }

        /// <summary>
        /// Removes <paramref name="suffixToRemove"/> from the end of <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input string to modify.</param>
        /// <param name="suffixToRemove">String to remove.</param>
        /// <param name="comparison">StringComparison type to use.</param>
        /// <returns>Modified <paramref name="input"/> string if it ended with <paramref name="suffixToRemove"/>; otherwise, <paramref name="input"/> is returned unmodified.</returns>
        public static string TrimStrEnd(this string input, string suffixToRemove, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!input.EndsWith(suffixToRemove, comparison))
                return input;

            return input.Remove(input.LastIndexOf(suffixToRemove, comparison));
        }

        /// <summary>
        /// Removes <paramref name="prefixToRemove"/> from the beginning of <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input string to modify.</param>
        /// <param name="prefixToRemove">String to remove.</param>
        /// <param name="comparison">StringComparison type to use.</param>
        /// <returns>Modified <paramref name="input"/> string if it began with <paramref name="prefixToRemove"/>; otherwise, <paramref name="input"/> is returned unmodified.</returns>
        public static string TrimStrStart(this string input, string prefixToRemove, StringComparison comparison = StringComparison.Ordinal)
        {
            if (!input.StartsWith(prefixToRemove, comparison))
                return input;

            return input.Remove(0, prefixToRemove.Length);
        }

        /// <summary>
        /// Gets the relative path from any DirectoryInfo/FileInfo object to the given DirectoryInfo/FileInfo object.
        /// </summary>
        /// <param name="to">The FileSystemInfo object to find the relative path to.</param>
        /// <param name="from">The FileSystemInfo object to find the relative path from.</param>
        /// <returns></returns>
        public static string GetRelativePathFrom(this FileSystemInfo to, FileSystemInfo from)
        {
            return from.GetRelativePathTo(to);
        }

        /// <summary>
        /// Gets the relative path to any DirectoryInfo/FileInfo object from the given DirectoryInfo/FileInfo object.
        /// </summary>
        /// <param name="from">The FileSystemInfo object to find the relative path from.</param>
        /// <param name="to">The FileSystemInfo object to find the relative path to.</param>
        /// <returns></returns>
        public static string GetRelativePathTo(this FileSystemInfo from, FileSystemInfo to)
        {
            string GetPath(FileSystemInfo fsi)
            {
                return (fsi is DirectoryInfo d) ? d.FullName.TrimEnd('\\') + "\\" : fsi.FullName;
            }

            string fromPath = GetPath(from);
            string toPath = GetPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Returns a list of processes running from the path specified by <paramref name="fileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">The executable to look for when enumerating running processes.</param>
        /// <returns>A list of Process instances that have a MainModule path matching <paramref name="fileInfo"/>.</returns>
        public static IEnumerable<Process> GetActiveProcesses(this FileInfo fileInfo)
        {
            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(fileInfo.Name));
        }

        ///// <summary>
        ///// Returns the command line string used to start the provided Process.
        ///// </summary>
        ///// <param name="process">The Process to query for its startup command line.</param>
        ///// <returns>A string containing the command line.</returns>
        //public static string GetCommandLine(this Process process)
        //{
        //    using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
        //    using (var objects = searcher.Get())
        //    {
        //        return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
        //    }
        //}

        ///// <summary>
        ///// Returns the command line string used to start the provided Process. If the command
        ///// line cannot be found for any reason, a null value is returned.
        ///// </summary>
        ///// <param name="process">The Process to query for its startup command line.</param>
        ///// <returns>A string containing the command line or null if a problem is encountered.</returns>
        //public static string GetCommandLineOrDefault(this Process process)
        //{
        //    try
        //    {
        //        return process.GetCommandLine();
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        /// <summary>
        /// A much more compact extension method that duplicates the functionality of 
        /// string.Equals(string1, string2, StringComparison.CurrentCultureIgnoreCase).
        /// If <paramref name="useOrdinalComparison"/> is true, 'OrdinalIgnoreCase' is 
        /// used instead of the default, 'CurrentCultureIgnoreCase'.
        /// </summary>
        /// <param name="value1">1st string value to compare.</param>
        /// <param name="value2">2nd string value to compare.</param>
        /// <param name="useOrdinalComparison">Use Ordinal comparison instead of CurrentCulture.</param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string value1, string value2, bool useOrdinalComparison = false)
        {
            return string.Equals(value1, value2,
                                 useOrdinalComparison
                                     ? StringComparison.OrdinalIgnoreCase
                                     : StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsInt(this string input)
        {
            int value;
            bool canConvert = int.TryParse(input, out value);
            return canConvert;
        }
    }
}
