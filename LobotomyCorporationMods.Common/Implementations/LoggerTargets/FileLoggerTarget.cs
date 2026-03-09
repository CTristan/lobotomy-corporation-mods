// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations.LoggerTargets
{
    public sealed class FileLoggerTarget : ILoggerTarget
    {
        private readonly IFileManager _fileManager;
        private readonly string _logFileName;

        public FileLoggerTarget(IFileManager fileManager,
            string logFileName)
        {
            _fileManager = fileManager;
            _logFileName = logFileName;
        }

        public void WriteToLoggerTarget(string message)
        {
            string logFile = _fileManager.GetFile(_logFileName);
            _fileManager.WriteAllText(logFile, message);
        }
    }
}
