// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class FileManager : IFileManager
    {
        private readonly DirectoryInfo _dataPath;
        private readonly object _fileLock = new();
        private readonly IDictionary<string, string> _filesCache;

        public FileManager(string modFileName, ICollection<DirectoryInfo> directories)
        {
            if (directories is null)
            {
                throw new ArgumentNullException(nameof(directories));
            }

            var directory = directories.FirstOrDefault(directoryInfo => File.Exists(Path.Combine(directoryInfo.FullName, modFileName)));

            if (directory is not null)
            {
                _dataPath = directory;

                var modFilePath = Path.Combine(_dataPath.FullName, modFileName);
                _filesCache = new Dictionary<string, string> { { modFileName, modFilePath } };
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Data path was not found, unable to find {modFileName} in the following directories:");
                foreach (var directoryInfo in directories)
                {
                    sb.AppendLine(directoryInfo.ToString());
                }

                throw new InvalidOperationException(sb.ToString());
            }
        }

        public string GetOrCreateFile(string fileName)
        {
            if (_filesCache.TryGetValue(fileName, out var value))
            {
                return value;
            }

            var fullFilePath = Path.Combine(_dataPath.FullName, fileName);
            _filesCache.Add(fileName, fullFilePath);

            return _filesCache[fileName];
        }

        public string ReadAllText(string fileWithPath, bool createIfNotExists)
        {
            if (!File.Exists(fileWithPath))
            {
                if (!createIfNotExists)
                {
                    return string.Empty;
                }

                WriteAllText(fileWithPath, string.Empty);
            }

            lock (_fileLock)
            {
                return File.ReadAllText(fileWithPath);
            }
        }

        public void WriteAllText(string fileWithPath, string contents)
        {
            lock (_fileLock)
            {
                File.WriteAllText(fileWithPath, contents);
            }
        }
    }
}
