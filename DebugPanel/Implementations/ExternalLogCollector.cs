// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class ExternalLogCollector : IInfoCollector<ExternalLogData>
    {
        private readonly IFileSystemScanner _scanner;

        public ExternalLogCollector(IFileSystemScanner scanner)
        {
            ThrowHelper.ThrowIfNull(scanner);
            _scanner = scanner;
        }

        public ExternalLogData Collect()
        {
            var gameRoot = _scanner.GetGameRootPath();
            var userProfile = _scanner.GetUserProfilePath();

            var retargetLog = ReadRetargetHarmonyLog(gameRoot);
            var bepInExLog = ReadBepInExLog(gameRoot);
            var unityLog = ReadUnityLog(userProfile);
            var gameplayLog = ReadGameplayLog(userProfile);
            var saveFolderLog = ReadSaveFolderLog(userProfile);
            var lmmDirectoryLog = ReadLmmDirectoryLog(gameRoot);
            var lmmSystemLog = ReadLmmSystemLog(userProfile);
            var baseModsLog = ReadBaseModsLog();

            return new ExternalLogData(retargetLog, bepInExLog, unityLog, gameplayLog, saveFolderLog, lmmDirectoryLog, lmmSystemLog, baseModsLog);
        }

        private string ReadRetargetHarmonyLog(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot))
            {
                return string.Empty;
            }

            try
            {
                var primaryPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(gameRoot, "BepInEx"), "patchers"), "RetargetHarmony"), "logs");
                primaryPath = Path.Combine(primaryPath, "RetargetHarmony.log");

                if (_scanner.FileExists(primaryPath))
                {
                    var content = _scanner.ReadAllText(primaryPath);
                    return string.IsNullOrEmpty(content) ? "(Log file exists but is empty — no warnings or errors recorded)" : content;
                }

                var fallbackPath = Path.Combine(Path.Combine(Path.Combine(gameRoot, "BepInEx"), "patchers"), "RetargetHarmony.log");

                if (_scanner.FileExists(fallbackPath))
                {
                    var content = _scanner.ReadAllText(fallbackPath);
                    return string.IsNullOrEmpty(content) ? "(Log file exists but is empty — no warnings or errors recorded)" : content;
                }
            }
            catch (Exception)
            {
                // Return empty if log can't be read
            }

            return string.Empty;
        }

        private string ReadBepInExLog(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot))
            {
                return string.Empty;
            }

            try
            {
                var logPath = Path.Combine(Path.Combine(gameRoot, "BepInEx"), "LogOutput.log");

                return _scanner.FileExists(logPath) ? _scanner.ReadLockedFile(logPath) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string ReadUnityLog(string userProfile)
        {
            return ReadUserProfileLog(userProfile, Path.Combine(Path.Combine(Path.Combine(Path.Combine("AppData", "LocalLow"), "Project_Moon"), "Lobotomy"), "output_log.txt"));
        }

        private string ReadGameplayLog(string userProfile)
        {
            return ReadUserProfileLog(userProfile, Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("AppData", "LocalLow"), "Project_Moon"), "Lobotomy"), "LobotomyBaseMod"), "Log.txt"));
        }

        private string ReadLmmSystemLog(string userProfile)
        {
            return ReadUserProfileLog(userProfile, Path.Combine(Path.Combine(Path.Combine(Path.Combine("AppData", "LocalLow"), "DefaultCompany"), "LobotomyModManager"), "Player.log"));
        }

        private string ReadUserProfileLog(string userProfile, string relativePath)
        {
            if (string.IsNullOrEmpty(userProfile))
            {
                return string.Empty;
            }

            try
            {
                var logPath = Path.Combine(userProfile, relativePath);

                return _scanner.FileExists(logPath) ? _scanner.ReadLockedFile(logPath) : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string ReadSaveFolderLog(string userProfile)
        {
            if (string.IsNullOrEmpty(userProfile))
            {
                return string.Empty;
            }

            try
            {
                var logDir = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(userProfile, "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "Log");
                var files = _scanner.GetFiles(logDir, "*.txt");

                if (files.Count == 0)
                {
                    return string.Empty;
                }

                var mostRecentPath = files[0];
                var mostRecentTime = _scanner.GetLastWriteTimeUtc(mostRecentPath);

                for (var i = 1; i < files.Count; i++)
                {
                    var writeTime = _scanner.GetLastWriteTimeUtc(files[i]);
                    if (writeTime > mostRecentTime)
                    {
                        mostRecentTime = writeTime;
                        mostRecentPath = files[i];
                    }
                }

                return _scanner.ReadLockedFile(mostRecentPath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string ReadLmmDirectoryLog(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot))
            {
                return string.Empty;
            }

            try
            {
                var parentDir = Path.GetDirectoryName(gameRoot);
                if (string.IsNullOrEmpty(parentDir))
                {
                    return string.Empty;
                }

                var directories = _scanner.GetDirectories(parentDir);
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName != null && dirName.StartsWith("LobotomyModManager", StringComparison.OrdinalIgnoreCase))
                    {
                        var logPath = Path.Combine(Path.Combine(dir, "LobotomyModManager_Data"), "Log.txt");
                        if (_scanner.FileExists(logPath))
                        {
                            return _scanner.ReadAllText(logPath);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty if LMM directory log can't be found
            }

            return string.Empty;
        }

        private string ReadBaseModsLog()
        {
            try
            {
                var baseModsPath = _scanner.GetBaseModsPath();
                var files = _scanner.GetFiles(baseModsPath, "*.txt");

                if (files.Count == 0)
                {
                    return string.Empty;
                }

                var combined = string.Empty;
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    if (_scanner.FileExists(file))
                    {
                        var content = _scanner.ReadAllText(file);
                        if (!string.IsNullOrEmpty(content))
                        {
                            if (!string.IsNullOrEmpty(combined))
                            {
                                combined += Environment.NewLine + Environment.NewLine;
                            }

                            combined += "--- " + fileName + " ---" + Environment.NewLine + content;
                        }
                    }
                }

                return combined;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
