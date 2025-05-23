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
        private readonly DirectoryInfo _dataPath;
        private readonly object _fileLock = new object();

        public FileManager([NotNull] string modFileName,
            [NotNull] ICollection<DirectoryInfo> directories)
        {
            Guard.Against.Null(directories, nameof(directories));

            var directory = directories.FirstOrDefault(directoryInfo =>
                File.Exists(Path.Combine(directoryInfo.FullName, modFileName)));

            if (directory.IsNotNull())
            {
                _dataPath = directory;
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

        public void AppendAllText(string filePath, string contents)
        {
            lock (_fileLock)
            {
                File.AppendAllText(filePath, contents);
            }
        }

        public string GetFullPathForFile([NotNull] string fileName)
        {
            return Path.Combine(_dataPath.FullName, fileName);
        }

        [NotNull]
        public byte[] ReadAllBytes([NotNull] string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        [NotNull]
        public string ReadAllText(string filePath,
            bool createIfNotExists)
        {
            lock (_fileLock)
            {
                if (!File.Exists(filePath) && createIfNotExists)
                {
                    File.WriteAllText(filePath, string.Empty);
                }

                return File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
            }
        }

        public void WriteAllText([NotNull] string filePath,
            string contents)
        {
            lock (_fileLock)
            {
                File.WriteAllText(filePath, contents);
            }
        }
    }
}
