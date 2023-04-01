// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.LoggerTargets
{
    public sealed class FileLoggerTarget : ILoggerTarget
    {
        private readonly IFileManager _fileManager;
        private readonly string _logFileName;

        public FileLoggerTarget(IFileManager fileManager, string logFileName)
        {
            _fileManager = fileManager;
            _logFileName = logFileName;
        }

        public void WriteToLoggerTarget(string message)
        {
            var logFile = _fileManager.GetOrCreateFile(_logFileName);
            _fileManager.WriteAllText(logFile, message);
        }
    }
}
