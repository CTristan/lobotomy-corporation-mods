// SPDX-License-Identifier: MIT

using System;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Logger : ILogger
    {
        private const string DefaultLogFileName = "log.txt";
        private readonly IFileManager _fileManager;

        public Logger([NotNull] string modFileName) : this(new FileManager(modFileName))
        {
        }

        public Logger(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public void WriteToLog(Exception ex)
        {
            WriteToLog(ex, DefaultLogFileName);
        }

        private void WriteToLog([NotNull] string message, [NotNull] string logFileName)
        {
            var logFile = _fileManager.GetFile(logFileName);
            _fileManager.WriteAllText(logFile, message);
        }

        private void WriteToLog([CanBeNull] Exception ex, [NotNull] string logFileName)
        {
            if (ex != null)
            {
                WriteToLog(ex.ToString(), logFileName);
            }
        }
    }
}
