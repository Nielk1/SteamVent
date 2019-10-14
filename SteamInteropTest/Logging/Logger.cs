using SteamVent.Common.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamInteropTest.Logging
{
    public class Logger : ILogger
    {
        public Logger()
        {
            if (Properties.Settings.Default.DebugLogEnabled)
                EnableLogFileOutput(Properties.Settings.Default.LogPath);
        }

        public void Error(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "ERROR", callerPath, memberName);
        }

        public void Error(Exception ex, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(ex.Message, indentLevel, "ERROR", callerPath, memberName);
        }

        public void Warning(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "WARNING", callerPath, memberName);
        }

        public void Info(string message, int indentLevel = 0, [CallerFilePath] string callerPath = null, [CallerMemberName] string memberName = null)
        {
            WriteEntry(message, indentLevel, "INFO", callerPath, memberName);
        }

        private static void WriteEntry(string message, int indentLevel, string type, string callerPath, string memberName)
        {
            var msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{type}]";
            if (!string.IsNullOrWhiteSpace(callerPath))
                msg += $" [{Path.GetFileNameWithoutExtension(callerPath)}]";
            if (!string.IsNullOrWhiteSpace(memberName))
                msg += $" [{memberName}]";

            msg += $" - {message}";

            Trace.IndentLevel = indentLevel;
            Trace.WriteLine(msg);
        }

        public void EnableLogFileOutput(string logFilePath, int maxFileLen = 1000000, int maxFileCount = 4, FileMode fileMode = FileMode.Append)
        {
            try
            {
                // Setup debug logging
                var fileStream = new FileStreamWithBackup(logFilePath, maxFileLen, maxFileCount, fileMode);
                var textWriter = new TextWriterTraceListener(fileStream);
                Trace.Listeners.Add(textWriter);
                Trace.AutoFlush = true;
            }
            catch (Exception ex)
            {
                this.Error($"An error occurred while attempting to enable log file output. Exception: {ex.Message}");
            }

        }
    }
}
