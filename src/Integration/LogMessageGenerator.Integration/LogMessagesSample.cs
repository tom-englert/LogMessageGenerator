using System;
using Microsoft.Extensions.Logging;

namespace LogMessageGenerator.Integration
{
    internal static class LogMessagesSample
    {
        private static Action<ILogger, string, Exception?> FILE_SKIPPED_Message = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "FILE_SKIPPED"), "Skip loading settings from {File}. File does not exist.");
        private static Action<ILogger, Exception?> FILE_SKIPPED_Message1 = LoggerMessage.Define(LogLevel.Information, new EventId(1, "FILE_SKIPPED"), "Skip loading settings from. File does not exist.");

        public static void Log_FILE_SKIPPED(this ILogger logger, string file, Exception? ex = null)
        {
            FILE_SKIPPED_Message(logger, file, ex);
        }
    }
}
