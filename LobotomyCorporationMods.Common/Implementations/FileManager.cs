// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class FileManager : IFileManager
    {
        private readonly DirectoryInfo _dataPath;
        [NotNull] private readonly object _fileLock = new object();
        [NotNull] private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        public FileManager([NotNull] string modFileName) : this(modFileName, Add_On.instance.DirList)
        {
        }

        private FileManager([NotNull] string modFileName, [NotNull] IEnumerable<DirectoryInfo> directories)
        {
            var directory = directories.FirstOrDefault(directoryInfo => File.Exists(Path.Combine(directoryInfo.FullName, modFileName)));
            _dataPath = directory ?? throw new InvalidOperationException("Data path was not found.");
        }

        public string GetFile([NotNull] string fileName)
        {
            if (_files.ContainsKey(fileName))
            {
                return _files[fileName];
            }

            var fullFilePath = Path.Combine(_dataPath.FullName, fileName);
            _files.Add(fileName, fullFilePath);

            return _files[fileName];
        }

        [NotNull]
        public string ReadAllText([NotNull] string path)
        {
            return ReadAllText(path, false);
        }

        [NotNull]
        public string ReadAllText([NotNull] string path, bool createIfNotExists)
        {
            if (!File.Exists(path))
            {
                if (!createIfNotExists)
                {
                    return string.Empty;
                }

                WriteAllText(path, string.Empty);
            }

            lock (_fileLock)
            {
                return File.ReadAllText(path);
            }
        }

        public void WriteAllText([NotNull] string path, [NotNull] string contents)
        {
            lock (_fileLock)
            {
                File.WriteAllText(path, contents);
            }
        }

        public void WriteToLog([NotNull] string message, [NotNull] string logFileName = "log.txt")
        {
            var logFile = Path.Combine(_dataPath.FullName, logFileName);
            WriteAllText(logFile, message);
        }

        public void WriteToLog([CanBeNull] Exception ex, [NotNull] string logFileName = "log.txt")
        {
            if (ex != null)
            {
                WriteToLog(ex.ToString(), logFileName);
            }
        }
    }
}
