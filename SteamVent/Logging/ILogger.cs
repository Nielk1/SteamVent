using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamVent.Logging
{
    public interface ILogger
    {
        void Error(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null);
        void Error(Exception ex, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null);
        void Warning(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null);
        void Info(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null);
        //public static void EnableLogFileOutput(string logFilePath, int maxFileLen = 1000000, int maxFileCount = 4, FileMode fileMode = FileMode.Append);
    }
}
