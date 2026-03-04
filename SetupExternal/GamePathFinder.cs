using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SetupExternal;

/// <summary>
/// Locates the Lobotomy Corporation game installation directory across different platforms.
/// </summary>
internal static class GamePathFinder
{
    private const string GameFolderName = "LobotomyCorp";
    private const string SteamAppsFolder = "steamapps";
    private const string CommonFolder = "common";
    private const string LibraryFoldersFile = "libraryfolders.vdf";

    /// <summary>
    /// Writes a debug message if debug logging is enabled.
    /// </summary>
    private static void DebugLog(string message)
    {
        Program.DebugLog($"[GamePathFinder] {message}");
    }

    /// <summary>
    /// Attempts to locate the game installation directory.
    /// </summary>
    /// <returns>The path to the game directory if found, or null if not found.</returns>
    public static string? FindGamePath()
    {
        DebugLog("Starting game path search");
        var candidates = GetCandidatePaths();
        DebugLog($"Found {candidates.Count} candidate paths to check");

        foreach (var candidate in candidates)
        {
            DebugLog($"Checking: {candidate}");
            if (IsValidGamePath(candidate))
            {
                DebugLog($"Found valid game path: {candidate}");
                return candidate;
            }
        }

        DebugLog("No valid game path found");
        return null;
    }

    /// <summary>
    /// Gets a list of candidate paths to search for the game installation.
    /// </summary>
    /// <returns>A list of candidate paths.</returns>
    private static List<string> GetCandidatePaths()
    {
        var candidates = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DebugLog("Detected Windows OS");
            candidates.AddRange(GetWindowsPaths());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            DebugLog("Detected Linux OS");
            candidates.AddRange(GetLinuxPaths());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            DebugLog("Detected macOS OS");
            candidates.AddRange(GetMacOSPaths());
        }

        return candidates;
    }

    /// <summary>
    /// Gets candidate game paths on Windows.
    /// </summary>
    private static List<string> GetWindowsPaths()
    {
        var paths = new List<string>();

        // Default Steam installation path
        var defaultSteamPath = @"C:\Program Files (x86)\Steam";
        paths.AddRange(GetSteamLibraryPaths(defaultSteamPath));

        // Fallback: check Windows Registry for Steam installation path
        var registrySteamPath = GetSteamPathFromRegistry();
        if (registrySteamPath != null && !registrySteamPath.Equals(defaultSteamPath, StringComparison.OrdinalIgnoreCase))
        {
            DebugLog($"Found Steam in Registry: {registrySteamPath}");
            paths.AddRange(GetSteamLibraryPaths(registrySteamPath));
        }

        return paths;
    }

    /// <summary>
    /// Attempts to locate Steam's installation path from the Windows Registry.
    /// </summary>
    /// <returns>The Steam installation path if found, or null if not found.</returns>
    private static string? GetSteamPathFromRegistry()
    {
        DebugLog("Attempting to find Steam path in Windows Registry");

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DebugLog("Skipping Registry check: not running on Windows");
            return null;
        }

        // Check HKLM first (system-wide install)
        DebugLog("Checking HKLM\\SOFTWARE\\Valve\\Steam for InstallPath");
        try
        {
            using (var hklmKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
            {
                if (hklmKey != null)
                {
                    var installPath = hklmKey.GetValue("InstallPath") as string;
                    DebugLog($"HKLM InstallPath value: {installPath ?? "(null)"}");

                    if (string.IsNullOrWhiteSpace(installPath))
                    {
                        DebugLog("HKLM InstallPath is null or whitespace");
                    }
                    else if (!Directory.Exists(installPath))
                    {
                        DebugLog($"HKLM InstallPath does not exist: {installPath}");
                    }
                    else
                    {
                        DebugLog($"Found valid Steam path in HKLM registry: {installPath}");
                        return installPath;
                    }
                }
                else
                {
                    DebugLog("HKLM\\SOFTWARE\\Valve\\Steam key does not exist");
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
        {
            DebugLog($"Access denied reading HKLM registry key: {ex.Message}");
        }

        // Check HKCU (per-user install)
        DebugLog("Checking HKCU\\SOFTWARE\\Valve\\Steam for InstallPath");
        try
        {
            using (var hkcuKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
            {
                if (hkcuKey != null)
                {
                    var installPath = hkcuKey.GetValue("InstallPath") as string;
                    DebugLog($"HKCU InstallPath value: {installPath ?? "(null)"}");

                    if (string.IsNullOrWhiteSpace(installPath))
                    {
                        DebugLog("HKCU InstallPath is null or whitespace");
                    }
                    else if (!Directory.Exists(installPath))
                    {
                        DebugLog($"HKCU InstallPath does not exist: {installPath}");
                    }
                    else
                    {
                        DebugLog($"Found valid Steam path in HKCU registry: {installPath}");
                        return installPath;
                    }
                }
                else
                {
                    DebugLog("HKCU\\SOFTWARE\\Valve\\Steam key does not exist");
                }
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
        {
            DebugLog($"Access denied reading HKCU registry key: {ex.Message}");
        }

        DebugLog("Steam path not found in registry");
        return null;
    }

    /// <summary>
    /// Gets candidate game paths on Linux.
    /// </summary>
    private static List<string> GetLinuxPaths()
    {
        var paths = new List<string>();

        // Common Steam locations on Linux
        var steamPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam")
        };

        foreach (var steamPath in steamPaths)
        {
            paths.AddRange(GetSteamLibraryPaths(steamPath));
        }

        return paths;
    }

    /// <summary>
    /// Gets candidate game paths on macOS (CrossOver bottles).
    /// </summary>
    private static List<string> GetMacOSPaths()
    {
        var paths = new List<string>();

        var bottlesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "Application Support",
            "CrossOver",
            "Bottles"
        );

        if (!Directory.Exists(bottlesPath))
        {
            DebugLog($"Bottles directory not found: {bottlesPath}");
            return paths;
        }

        DebugLog($"Searching bottles in: {bottlesPath}");

        // Search all bottles for the game directory
        foreach (var bottleDir in Directory.GetDirectories(bottlesPath))
        {
            var driveC = Path.Combine(bottleDir, "drive_c");
            if (!Directory.Exists(driveC))
            {
                continue;
            }

            DebugLog($"Checking bottle: {bottleDir}");

            // Check for Steam installation in bottle
            var steamPath = Path.Combine(driveC, "Program Files (x86)", "Steam");
            if (Directory.Exists(steamPath))
            {
                // Parse libraryfolders.vdf to find Steam libraries (including external drives like Z:)
                DebugLog($"Steam installation found in bottle: {steamPath}");
                var libraryPaths = GetSteamLibraryPaths(steamPath);
                paths.AddRange(libraryPaths);
            }

            // Fallback to recursive search for game directory
            var gamePath = FindGamePathRecursive(driveC);
            if (gamePath != null)
            {
                paths.Add(gamePath);
            }
        }

        DebugLog($"Found {paths.Count} candidate paths from macOS bottles");
        return paths;
    }

    /// <summary>
    /// Recursively searches for the game directory in a given root path.
    /// </summary>
    private static string? FindGamePathRecursive(string rootPath)
    {
        try
        {
            // Check if current directory is the game directory
            if (IsValidGamePath(rootPath))
            {
                return rootPath;
            }

            // Recursively search subdirectories (limit depth to avoid scanning entire drive)
            var subdirs = Directory.GetDirectories(rootPath);
            foreach (var subdir in subdirs.Take(10)) // Limit to first 10 subdirs per level
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

    /// <summary>
    /// Converts CrossOver Windows-style paths to macOS Unix paths.
    /// Example: "Z:\\Volumes\WD_BLACK SN770 1TB\SteamLibrary" → "/Volumes/WD_BLACK SN770 1TB/SteamLibrary"
    /// </summary>
    private static string? ConvertCrossOverPath(string windowsPath)
    {
        // CrossOver external drives typically start with Z:, X:, etc.
        if (windowsPath.StartsWith("Z:\\\\", StringComparison.Ordinal))
        {
            // Remove "Z:\\" and convert all backslashes to forward slashes
            var unixPath = windowsPath.Substring(4).Replace("\\\\", "/", StringComparison.Ordinal);
            return "/" + unixPath;
        }

        // Other drive letters could be added here (C:, D:, etc.)
        // For now, return null for unsupported drives
        return null;
    }

    /// <summary>
    /// Gets Steam library paths from a Steam installation directory.
    /// </summary>
    private static List<string> GetSteamLibraryPaths(string steamPath)
    {
        var paths = new List<string>();

        if (!Directory.Exists(steamPath))
        {
            return paths;
        }

        // Parse libraryfolders.vdf to find Steam libraries
        var libraryFoldersPath = Path.Combine(steamPath, SteamAppsFolder, LibraryFoldersFile);
        if (File.Exists(libraryFoldersPath))
        {
            try
            {
                var vdfContent = File.ReadAllText(libraryFoldersPath);
                var libraryPaths = VdfParser.ExtractLibraryPaths(vdfContent);

                foreach (var libRootPath in libraryPaths)
                {
                    // Convert CrossOver Windows-style paths to macOS Unix paths
                    var convertedPath = ConvertCrossOverPath(libRootPath) ?? libRootPath;
                    // The VDF path is already the Steam library root, so just append the game folder
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
            // No libraryfolders.vdf, add default path
            var defaultGamePath = Path.Combine(steamPath, SteamAppsFolder, CommonFolder, GameFolderName);
            paths.Add(defaultGamePath);
        }

        return paths;
    }

    /// <summary>
    /// Validates that a path is a valid Lobotomy Corporation installation.
    /// </summary>
    private static bool IsValidGamePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var assemblyCSharpPath = Path.Combine(path, "LobotomyCorp_Data", "Managed", "Assembly-CSharp.dll");
        var isValid = File.Exists(assemblyCSharpPath);

        DebugLog($"Validating path: {path}");
        DebugLog($"  Looking for: {assemblyCSharpPath}");
        DebugLog($"  Found: {isValid}");

        return isValid;
    }
}
