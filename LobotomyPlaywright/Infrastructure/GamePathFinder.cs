// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Infrastructure;

using LobotomyPlaywright.Infrastructure;

/// <summary>
/// Locates the Lobotomy Corporation game installation directory across different platforms.
/// </summary>
internal class GamePathFinder
{
    private const string GameFolderName = "LobotomyCorp";
    private const string SteamAppsFolder = "steamapps";
    private const string CommonFolder = "common";
    private const string LibraryFoldersFile = "libraryfolders.vdf";
    private readonly IFileSystem _fileSystem;
    private readonly Action<string>? _logCallback;

    /// <summary>
    /// Initializes a new instance of the GamePathFinder class.
    /// </summary>
    /// <param name="fileSystem">The file system implementation.</param>
    /// <param name="logCallback">Optional callback for debug logging.</param>
    public GamePathFinder(IFileSystem fileSystem, Action<string>? logCallback = null)
    {
        _fileSystem = fileSystem;
        _logCallback = logCallback;
    }

    /// <summary>
    /// Initializes a new instance of the GamePathFinder class with default file system.
    /// </summary>
    public GamePathFinder()
        : this(new FileSystem())
    {
    }

    /// <summary>
    /// Attempts to locate the game installation directory.
    /// </summary>
    /// <returns>A tuple of (game_path, bottle_name) if found, or null if not found.</returns>
    public (string GamePath, string? BottleName)? FindGamePath()
    {
        Log("Starting game path search");
        var candidates = GetCandidatePaths();
        Log($"Found {candidates.Count} candidate paths to check");

        foreach (var (gamePath, bottleName) in candidates)
        {
            Log($"Checking: {gamePath}");
            if (IsValidGamePath(gamePath))
            {
                Log($"Found valid game path: {gamePath}");
                return (gamePath, bottleName);
            }
        }

        Log("No valid game path found");
        return null;
    }

    /// <summary>
    /// Validates that a path is a valid Lobotomy Corporation installation.
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns>True if this is a valid game installation.</returns>
    public bool IsValidGamePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var assemblyCSharpPath = Path.Combine(path, "LobotomyCorp_Data", "Managed", "Assembly-CSharp.dll");
        var isValid = _fileSystem.FileExists(assemblyCSharpPath);

        Log($"Validating path: {path}");
        Log($"  Looking for: {assemblyCSharpPath}");
        Log($"  Found: {isValid}");

        return isValid;
    }

    /// <summary>
    /// Checks if BepInEx is installed at the given game path.
    /// </summary>
    /// <param name="gamePath">Game installation path.</param>
    /// <returns>True if BepInEx is installed.</returns>
    public bool IsBepInExInstalled(string gamePath)
    {
        var bepinexDll = Path.Combine(gamePath, "BepInEx", "core", "BepInEx.dll");
        var doorstopConfig = Path.Combine(gamePath, "doorstop_config.ini");
        return _fileSystem.FileExists(bepinexDll) && _fileSystem.FileExists(doorstopConfig);
    }

    private List<(string GamePath, string? BottleName)> GetCandidatePaths()
    {
        var candidates = new List<(string, string?)>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Log("Detected Windows OS");
            candidates.AddRange(GetWindowsPaths());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Log("Detected Linux OS");
            candidates.AddRange(GetLinuxPaths());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Log("Detected macOS OS");
            candidates.AddRange(GetMacOSPaths());
        }

        return candidates;
    }

    private List<(string, string?)> GetWindowsPaths()
    {
        var paths = new List<(string, string?)>();

        // Default Steam installation path
        var defaultSteamPath = @"C:\Program Files (x86)\Steam";
        paths.AddRange(GetSteamLibraryPaths(defaultSteamPath, null));

        // Fallback: check Windows Registry for Steam installation path
        var registrySteamPath = GetSteamPathFromRegistry();
        if (registrySteamPath != null && !registrySteamPath.Equals(defaultSteamPath, StringComparison.OrdinalIgnoreCase))
        {
            Log($"Found Steam in Registry: {registrySteamPath}");
            paths.AddRange(GetSteamLibraryPaths(registrySteamPath, null));
        }

        return paths;
    }

    private string? GetSteamPathFromRegistry()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return null;
        }

        try
        {
            using var hklmKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (hklmKey != null)
            {
                var installPath = hklmKey.GetValue("InstallPath") as string;
                if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
                {
                    Log($"Found valid Steam path in HKLM registry: {installPath}");
                    return installPath;
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
        {
            Log($"Access denied reading HKLM registry key: {ex.Message}");
        }

        try
        {
            using var hkcuKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (hkcuKey != null)
            {
                var installPath = hkcuKey.GetValue("InstallPath") as string;
                if (!string.IsNullOrWhiteSpace(installPath) && Directory.Exists(installPath))
                {
                    Log($"Found valid Steam path in HKCU registry: {installPath}");
                    return installPath;
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
        {
            Log($"Access denied reading HKCU registry key: {ex.Message}");
        }

        return null;
    }

    private List<(string, string?)> GetLinuxPaths()
    {
        var paths = new List<(string, string?)>();

        var steamPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam")
        };

        foreach (var steamPath in steamPaths)
        {
            paths.AddRange(GetSteamLibraryPaths(steamPath, null));
        }

        return paths;
    }

    private List<(string, string?)> GetMacOSPaths()
    {
        var paths = new List<(string, string?)>();

        var bottlesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "Application Support",
            "CrossOver",
            "Bottles"
        );

        if (!_fileSystem.DirectoryExists(bottlesPath))
        {
            Log($"Bottles directory not found: {bottlesPath}");
            return paths;
        }

        Log($"Searching bottles in: {bottlesPath}");

        foreach (var bottleDir in Directory.GetDirectories(bottlesPath))
        {
            var bottleName = Path.GetFileName(bottleDir);
            var driveC = Path.Combine(bottleDir, "drive_c");
            if (!_fileSystem.DirectoryExists(driveC))
            {
                continue;
            }

            Log($"Checking bottle: {bottleName}");

            // Check for Steam installation in bottle
            var steamPath = Path.Combine(driveC, "Program Files (x86)", "Steam");
            if (_fileSystem.DirectoryExists(steamPath))
            {
                Log($"  Steam installation found in bottle: {steamPath}");
                paths.AddRange(GetSteamLibraryPaths(steamPath, bottleName));
            }

            // Fallback to recursive search for game directory
            var gamePath = FindGamePathRecursive(driveC);
            if (gamePath != null)
            {
                paths.Add((gamePath, bottleName));
            }
        }

        Log($"Found {paths.Count} candidate paths from macOS bottles");
        return paths;
    }

    private string? FindGamePathRecursive(string rootPath)
    {
        if (IsValidGamePath(rootPath))
        {
            return rootPath;
        }

        try
        {
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

    private List<(string, string?)> GetSteamLibraryPaths(string steamPath, string? bottleName)
    {
        var paths = new List<(string, string?)>();

        if (!_fileSystem.DirectoryExists(steamPath))
        {
            return paths;
        }

        var libraryFoldersPath = Path.Combine(steamPath, SteamAppsFolder, LibraryFoldersFile);
        if (_fileSystem.FileExists(libraryFoldersPath))
        {
            try
            {
                var vdfContent = _fileSystem.ReadAllText(libraryFoldersPath);
                if (vdfContent != null)
                {
                    var libraryPaths = VdfParser.ExtractLibraryPaths(vdfContent);

                    foreach (var libRootPath in libraryPaths)
                    {
                        var convertedPath = ConvertCrossOverPath(libRootPath) ?? libRootPath;
                        var gamePath = Path.Combine(convertedPath, SteamAppsFolder, CommonFolder, GameFolderName);
                        paths.Add((gamePath, bottleName));
                    }
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
            paths.Add((defaultGamePath, bottleName));
        }

        return paths;
    }

    private string? ConvertCrossOverPath(string windowsPath)
    {
        if (windowsPath.StartsWith("Z:\\", StringComparison.Ordinal))
        {
            var unixPath = windowsPath.Substring(3).Replace("\\", "/", StringComparison.Ordinal);
            return "/" + unixPath;
        }

        return null;
    }

    private void Log(string message)
    {
        _logCallback?.Invoke($"[GamePathFinder] {message}");
    }
}
