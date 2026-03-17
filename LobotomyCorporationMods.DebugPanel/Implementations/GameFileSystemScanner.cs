// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class GameFileSystemScanner : IFileSystemScanner
    {
        private readonly string _gameRoot;
        private readonly string _dataPath;

        public GameFileSystemScanner()
        {
            _dataPath = Application.dataPath;
            _gameRoot = Path.GetDirectoryName(_dataPath) ?? string.Empty;
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public IList<string> GetFiles(string directoryPath, string searchPattern)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return new List<string>();
                }

                return new List<string>(Directory.GetFiles(directoryPath, searchPattern));
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public IList<string> GetDirectories(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return new List<string>();
                }

                return new List<string>(Directory.GetDirectories(path));
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public string GetBaseModsPath()
        {
            return Path.Combine(_dataPath, "BaseMods");
        }

        public string GetSaveDataPath()
        {
            var persistentDataPath = Application.persistentDataPath;

            return Path.Combine(persistentDataPath, "Project_Moon");
        }

        public string GetGameRootPath()
        {
            return _gameRoot;
        }

        public string GetExternalDataPath()
        {
            return Path.Combine(Path.Combine(Path.Combine(_dataPath, "BaseMods"), "LobotomyCorporationMods.DebugPanel"), "Data");
        }
    }
}
