// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;
using UnityEngine;

#endregion

namespace DebugPanel.Implementations
{
    /// <summary>
    ///     Accesses BepInEx shim artifacts on the filesystem: the backup directory and interop cache file.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class FileSystemShimArtifactSource : IShimArtifactSource
    {
        private const string BackupDirectoryName = "BepInEx_Shim_Backup";
        private const string CacheRelativePath = "BepInEx/cache/harmony_interop_cache.dat";

        private readonly string _gameRoot;

        public FileSystemShimArtifactSource()
        {
            // Application.dataPath returns "{gameRoot}/LobotomyCorp_Data"
            var dataPath = Application.dataPath;
            _gameRoot = Path.GetDirectoryName(dataPath) ?? string.Empty;
        }

        public bool BackupDirectoryExists => Directory.Exists(GetBackupPath());

        public string BackupDirectoryPath => GetBackupPath();

        public bool InteropCacheExists => File.Exists(GetCachePath());

        public string InteropCachePath => GetCachePath();

        public IList<string> GetBackupFileNames()
        {
            try
            {
                if (!BackupDirectoryExists)
                {
                    return new List<string>();
                }

                var files = Directory.GetFiles(GetBackupPath());
                var names = new List<string>(files.Length);

                foreach (var file in files)
                {
                    names.Add(Path.GetFileName(file));
                }

                return names;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public byte[] ReadBackupFileBytes(string fileName)
        {
            try
            {
                var fullPath = Path.Combine(GetBackupPath(), fileName);

                return File.ReadAllBytes(fullPath);
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }

        public int GetInteropCacheEntryCount()
        {
            try
            {
                if (!InteropCacheExists)
                {
                    return -1;
                }

                var count = 0;

                using (var stream = File.OpenRead(GetCachePath()))
                using (var reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length)
                    {
                        _ = reader.ReadString();
                        _ = reader.ReadInt64();
                        count++;
                    }
                }

                return count;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private string GetBackupPath()
        {
            return Path.Combine(_gameRoot, BackupDirectoryName);
        }

        private string GetCachePath()
        {
            return Path.Combine(_gameRoot, CacheRelativePath);
        }
    }
}
