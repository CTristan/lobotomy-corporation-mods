// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using RetargetHarmony.Installer.Interfaces;

namespace RetargetHarmony.Installer.Services
{
    /// <summary>
    /// Locates the Lobotomy Corporation game installation directory across different platforms.
    /// Adapted from SetupExternal/GamePathFinder.cs for use in the installer.
    /// </summary>
    public sealed class GamePathFinder : IGamePathFinder
    {
        private const string GameFolderName = "LobotomyCorp";
        private const string SteamAppsFolder = "steamapps";
        private const string CommonFolder = "common";
        private const string LibraryFoldersFile = "libraryfolders.vdf";

        /// <inheritdoc />
        public string? FindGamePath()
        {
            var candidates = GetCandidatePaths();

            foreach (var candidate in candidates)
            {
                if (IsValidGamePath(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static List<string> GetCandidatePaths()
        {
            List<string> candidates = [];

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                candidates.AddRange(GetWindowsPaths());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                candidates.AddRange(GetLinuxPaths());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                candidates.AddRange(GetMacOSPaths());
            }

            return candidates;
        }

        private static List<string> GetWindowsPaths()
        {
            List<string> paths = [];

            var defaultSteamPath = @"C:\Program Files (x86)\Steam";
            paths.AddRange(GetSteamLibraryPaths(defaultSteamPath));

            var registrySteamPath = GetSteamPathFromRegistry();
            if (registrySteamPath != null && !registrySteamPath.Equals(defaultSteamPath, StringComparison.OrdinalIgnoreCase))
            {
                paths.AddRange(GetSteamLibraryPaths(registrySteamPath));
            }

            return paths;
        }

        private static string? GetSteamPathFromRegistry()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            try
            {
                using var hklmKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (hklmKey != null)
                {
                    var installPath = hklmKey.GetValue("InstallPath") as string;
                    if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
                    {
                        return installPath;
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
            {
                // Access denied, continue to next check
            }

            try
            {
                using var hkcuKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (hkcuKey != null)
                {
                    var installPath = hkcuKey.GetValue("InstallPath") as string;
                    if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
                    {
                        return installPath;
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
            {
                // Access denied
            }

            return null;
        }

        private static List<string> GetLinuxPaths()
        {
            List<string> paths = [];

            string[] steamPaths =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam"),
            ];

            foreach (var steamPath in steamPaths)
            {
                paths.AddRange(GetSteamLibraryPaths(steamPath));
            }

            return paths;
        }

        private static List<string> GetMacOSPaths()
        {
            List<string> paths = [];

            var bottlesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Application Support",
                "CrossOver",
                "Bottles");

            if (!Directory.Exists(bottlesPath))
            {
                return paths;
            }

            foreach (var bottleDir in Directory.GetDirectories(bottlesPath))
            {
                var driveC = Path.Combine(bottleDir, "drive_c");
                if (!Directory.Exists(driveC))
                {
                    continue;
                }

                var steamPath = Path.Combine(driveC, "Program Files (x86)", "Steam");
                if (Directory.Exists(steamPath))
                {
                    var libraryPaths = GetSteamLibraryPaths(steamPath);
                    paths.AddRange(libraryPaths);
                }

                var gamePath = FindGamePathRecursive(driveC);
                if (gamePath != null)
                {
                    paths.Add(gamePath);
                }
            }

            return paths;
        }

        private static string? FindGamePathRecursive(string rootPath)
        {
            try
            {
                if (IsValidGamePath(rootPath))
                {
                    return rootPath;
                }

                var subdirs = Directory.GetDirectories(rootPath);
                foreach (var subdir in subdirs.Take(10))
                {
                    var result = FindGamePathRecursive(subdir);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }

            return null;
        }

        private static string? ConvertCrossOverPath(string windowsPath)
        {
            if (windowsPath.StartsWith("Z:\\\\", StringComparison.Ordinal))
            {
                var unixPath = windowsPath[4..].Replace("\\\\", "/", StringComparison.Ordinal);
                return "/" + unixPath;
            }

            return null;
        }

        private static List<string> GetSteamLibraryPaths(string steamPath)
        {
            List<string> paths = [];

            if (!Directory.Exists(steamPath))
            {
                return paths;
            }

            var libraryFoldersPath = Path.Combine(steamPath, SteamAppsFolder, LibraryFoldersFile);
            if (File.Exists(libraryFoldersPath))
            {
                try
                {
                    var vdfContent = File.ReadAllText(libraryFoldersPath);
                    var libraryPaths = VdfParser.ExtractLibraryPaths(vdfContent);

                    foreach (var libRootPath in libraryPaths)
                    {
                        var convertedPath = ConvertCrossOverPath(libRootPath) ?? libRootPath;
                        var gamePath = Path.Combine(convertedPath, SteamAppsFolder, CommonFolder, GameFolderName);
                        paths.Add(gamePath);
                    }
                }
                catch (IOException)
                {
                    // Ignore errors reading the VDF file
                }
            }
            else
            {
                var defaultGamePath = Path.Combine(steamPath, SteamAppsFolder, CommonFolder, GameFolderName);
                paths.Add(defaultGamePath);
            }

            return paths;
        }

        private static bool IsValidGamePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var assemblyCSharpPath = Path.Combine(path, "LobotomyCorp_Data", "Managed", "Assembly-CSharp.dll");
            return File.Exists(assemblyCSharpPath);
        }
    }
}
