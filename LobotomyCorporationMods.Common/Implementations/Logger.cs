// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Logger : ILogger
    {
        private const string DefaultLogFileName = "log.txt";
        private readonly IAngelaConversationUiAdapter _angelaConversationUiAdapter;
        private readonly IFileManager _fileManager;

        public Logger(IFileManager fileManager)
            : this(fileManager, new AngelaConversationUiAdapter())
        {
        }

        public Logger(IFileManager fileManager, IAngelaConversationUiAdapter angelaConversationUiAdapter)
        {
            _fileManager = fileManager;
            _angelaConversationUiAdapter = angelaConversationUiAdapter;

#if DEBUG
            DebugLoggingEnabled = true;
#endif
        }

        public bool DebugLoggingEnabled { get; set; }

        public void WriteToLog(Exception exception)
        {
            WriteToLog(exception, DefaultLogFileName);
        }

        private void WriteToDebug(string message)
        {
            Notice.instance.Send(NoticeName.AddSystemLog, message);
            _angelaConversationUiAdapter.AddMessage(message);
        }

        private void WriteToLog(string message, string logFileName)
        {
            var logFile = _fileManager.GetOrCreateFile(logFileName);
            _fileManager.WriteAllText(logFile, message);
        }

        private void WriteToLog(Exception exception, string logFileName)
        {
            var message = exception.ToString();
            WriteToLog(message, logFileName);

            if (DebugLoggingEnabled)
            {
                WriteToDebug(message);
            }
        }
    }
}
