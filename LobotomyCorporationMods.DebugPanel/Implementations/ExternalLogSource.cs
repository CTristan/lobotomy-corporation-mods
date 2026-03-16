// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class ExternalLogSource : IExternalLogSource
    {
        public ExternalLogData GetExternalLogs()
        {
            var gameRoot = FindGameRoot();
            var retargetLog = ReadRetargetHarmonyLog(gameRoot);
            var bepInExLog = ReadBepInExLog(gameRoot);
            var unityLog = ReadUnityLog();

            return new ExternalLogData(retargetLog, bepInExLog, unityLog);
        }

        private static string FindGameRoot()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    try
                    {
                        var location = assembly.Location;
                        if (string.IsNullOrEmpty(location))
                        {
                            continue;
                        }

                        var dir = Path.GetDirectoryName(location);
                        if (string.IsNullOrEmpty(dir))
                        {
                            continue;
                        }

                        var current = dir;
                        for (var i = 0; i < 10; i++)
                        {
                            if (Directory.Exists(Path.Combine(current, "BepInEx")) ||
                                Directory.Exists(Path.Combine(current, "LobotomyCorp_Data")))
                            {
                                return current;
                            }

                            var parent = Path.GetDirectoryName(current);
                            if (string.IsNullOrEmpty(parent) || parent == current)
                            {
                                break;
                            }

                            current = parent;
                        }
                    }
                    catch (Exception)
                    {
                        // Skip assemblies that can't be inspected
                    }
                }
            }
            catch (Exception)
            {
                // Return empty if game root can't be found
            }

            return string.Empty;
        }

        private static string ReadRetargetHarmonyLog(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot))
            {
                return string.Empty;
            }

            var searchPaths = new[]
            {
                Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(gameRoot, "BepInEx"), "patchers"), "RetargetHarmony"), "logs"), "RetargetHarmony.log"),
                Path.Combine(Path.Combine(Path.Combine(gameRoot, "BepInEx"), "patchers"), "RetargetHarmony.log"),
            };

            foreach (var path in searchPaths)
            {
                var content = TryReadFile(path);
                if (!string.IsNullOrEmpty(content))
                {
                    return content;
                }
            }

            return string.Empty;
        }

        private static string ReadBepInExLog(string gameRoot)
        {
            if (string.IsNullOrEmpty(gameRoot))
            {
                return string.Empty;
            }

            var logPath = Path.Combine(Path.Combine(gameRoot, "BepInEx"), "LogOutput.log");

            return ReadLockedFile(logPath);
        }

        private static string ReadUnityLog()
        {
            try
            {
                var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                if (string.IsNullOrEmpty(userProfile))
                {
                    userProfile = Environment.GetEnvironmentVariable("HOME");
                }

                if (string.IsNullOrEmpty(userProfile))
                {
                    return string.Empty;
                }

                var logPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(userProfile, "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "output_log.txt");

                return ReadLockedFile(logPath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string TryReadFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            catch (Exception)
            {
                // Return empty if file can't be read
            }

            return string.Empty;
        }

        private static string ReadLockedFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return string.Empty;
                }

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
