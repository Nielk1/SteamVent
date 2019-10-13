using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamVent.Common.Logging
{
    public static class Logger
    {
        public static ILogger Log { get; set; }

        public static void Error(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            Log?.Error(message, indentLevel, callerPath, memberName);
        }

        public static void Error(Exception ex, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            Log?.Error(ex, indentLevel, callerPath, memberName);
        }

        public static void Warning(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            Log?.Warning(message, indentLevel, callerPath, memberName);
        }

        public static void Info(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            Log?.Info(message, indentLevel, callerPath, memberName);
        }
    }
}
