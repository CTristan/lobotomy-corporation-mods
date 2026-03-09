// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class FileManager : IFileManager
    {
        private readonly IDirectoryInfo _dataPath;
        private readonly object _fileLock = new object();
        private readonly Dictionary<string, string> _filesCache;

        public FileManager([NotNull] string modFileName,
            [NotNull] ICollection<IDirectoryInfo> directories)
        {
            _ = Guard.Against.Null(directories, nameof(directories));

            IDirectoryInfo directory = directories.FirstOrDefault(directoryInfo => File.Exists(Path.Combine(directoryInfo.FullName, modFileName)));

            if (directory.IsNotNull())
            {
                _dataPath = directory;

                string modFilePath = Path.Combine(_dataPath.FullName, modFileName);
                _filesCache = new Dictionary<string, string>
                {
                    {
                        modFileName, modFilePath
                    },
                };
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                _ = sb.AppendLine($"Data path was not found, unable to find {modFileName} in the following directories:");
                foreach (IDirectoryInfo directoryInfo in directories)
                {
                    _ = sb.AppendLine(directoryInfo.FullName);
                }

                throw new InvalidOperationException(sb.ToString());
            }
        }

        public string GetFile([NotNull] string fileName)
        {
            if (_filesCache.TryGetValue(fileName, out string value))
            {
                return value;
            }

            string fullFilePath = Path.Combine(_dataPath.FullName, fileName);
            _filesCache.Add(fileName, fullFilePath);

            return _filesCache[fileName];
        }

        [NotNull]
        public byte[] ReadAllBytes([NotNull] string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        [NotNull]
        public string ReadAllText(string fileWithPath,
            bool createIfNotExists)
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

        public void WriteAllText([NotNull] string fileWithPath,
            string contents)
        {
            lock (_fileLock)
            {
                File.WriteAllText(fileWithPath, contents);
            }
        }
    }
}
