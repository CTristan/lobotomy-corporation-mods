// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    internal sealed class FileManager : IFileManager
    {
        private readonly DirectoryInfo _dataPath;
        private readonly object _fileLock = new();
        private readonly Dictionary<string, string> _files = new();

        internal FileManager([NotNull] string modFileName, [NotNull] IEnumerable<DirectoryInfo> directories)
        {
            var directory = directories.FirstOrDefault(directoryInfo => File.Exists(Path.Combine(directoryInfo.FullName, modFileName)));

            if (directory is not null)
            {
                _dataPath = directory;
            }
            else
            {
                var sb = new StringBuilder($"Data path was not found, unable to find {modFileName} in the following directories:/n");
                foreach (var directoryInfo in directories)
                {
                    sb.AppendLine(directoryInfo.ToString());
                }

                throw new InvalidOperationException(sb.ToString());
            }
        }

        public string GetOrCreateFile([NotNull] string fileName)
        {
            if (_files.TryGetValue(fileName, out var value))
            {
                return value;
            }

            var fullFilePath = Path.Combine(_dataPath.FullName, fileName);
            _files.Add(fileName, fullFilePath);

            return _files[fileName];
        }

        public string GetFileIfExists([NotNull] string fileName)
        {
            if (_files.TryGetValue(fileName, out var value))
            {
                return value;
            }

            var fullFilePath = Path.Combine(_dataPath.FullName, fileName);
            var fileIfExists = "";

            if (File.Exists(fullFilePath))
            {
                _files.Add(fileName, fullFilePath);

                fileIfExists = _files[fileName];
            }

            return fileIfExists;
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
    }
}
