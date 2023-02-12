// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    internal sealed class Logger : ILogger
    {
        private const string DefaultLogFileName = "log.txt";
        private readonly IFileManager _fileManager;

        internal Logger(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void WriteToLog(Exception exception)
        {
            WriteToLog(exception, DefaultLogFileName);
        }

        private static void WriteToDebug(string message)
        {
            Notice.instance.Send(NoticeName.AddSystemLog, message);
            AngelaConversationUI.instance.AddAngelaMessage(message);
        }

        private void WriteToLog([NotNull] string message, [NotNull] string logFileName)
        {
            var logFile = _fileManager.GetOrCreateFile(logFileName);
            _fileManager.WriteAllText(logFile, message);
        }

        private void WriteToLog([CanBeNull] Exception exception, [NotNull] string logFileName)
        {
            if (exception != null)
            {
                var message = exception.ToString();
                WriteToLog(message, logFileName);

#if DEBUG
                WriteToDebug(message);
#endif
            }
        }
    }
}
