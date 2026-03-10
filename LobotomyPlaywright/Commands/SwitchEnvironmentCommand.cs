// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Linq;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to switch between debug and release game environments by swapping UnityPlayer.dll.
/// </summary>
public class SwitchEnvironmentCommand
{
    private readonly IConfigManager _configManager;
    private readonly IFileSystem _fileSystem;
    private readonly ProcessManager _processManager;

    /// <summary>
    /// Initializes a new instance of SwitchEnvironmentCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="processManager">The process manager.</param>
    public SwitchEnvironmentCommand(IConfigManager configManager, IFileSystem fileSystem, ProcessManager processManager)
    {
        _configManager = configManager;
        _fileSystem = fileSystem;
        _processManager = processManager;
    }

    /// <summary>
    /// Initializes a new instance of SwitchEnvironmentCommand class with default implementations.
    /// </summary>
    public SwitchEnvironmentCommand()
        : this(new ConfigManager(new FileSystem()), new FileSystem(), new ProcessManager())
    {
    }

    /// <summary>
    /// Runs the switch-environment command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var isStatus = HasArg(args, "--status");

        // Parse target environment
        string? target = null;
        if (!isStatus)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            target = args[0].ToLowerInvariant();
            if (target != "debug" && target != "release")
            {
                Console.Error.WriteLine($"ERROR: Invalid environment '{target}'. Use 'debug' or 'release'.");
                PrintUsage();
                return 1;
            }
        }

        // Load configuration
        Config config;
        try
        {
            config = _configManager.Load();
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }

        // Validate game path
        if (!_fileSystem.DirectoryExists(config.GamePath))
        {
            Console.Error.WriteLine($"ERROR: Game path does not exist: {config.GamePath}");
            return 1;
        }

        // Resolve DLL paths
        var gamePath = config.GamePath;
        var activeDllPath = Path.Combine(gamePath, "UnityPlayer.dll");
        var debugDllPath = Path.Combine(gamePath, "UnityPlayer_debug.dll");
        var releaseDllPath = Path.Combine(gamePath, "UnityPlayer_release.dll");

        // Validate variant files exist
        if (!_fileSystem.FileExists(debugDllPath))
        {
            Console.Error.WriteLine($"ERROR: Debug DLL not found: {debugDllPath}");
            return 1;
        }

        if (!_fileSystem.FileExists(releaseDllPath))
        {
            Console.Error.WriteLine($"ERROR: Release DLL not found: {releaseDllPath}");
            return 1;
        }

        // Detect current environment
        var current = DetectCurrentEnvironment(activeDllPath, debugDllPath, releaseDllPath);

        if (current == null)
        {
            Console.Error.WriteLine("ERROR: Cannot determine current environment - active DLL does not match either variant");
            return 1;
        }

        Console.WriteLine($"Current environment: {current}");

        // If status flag, just report and exit
        if (isStatus)
        {
            return 0;
        }

        // If already in target mode, no action needed
        if (target == current)
        {
            Console.WriteLine($"Already in {target} mode. No action needed.");
            return 0;
        }

        // Auto-stop game if running
        var pids = _processManager.FindGameProcesses();
        if (pids.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Game is running. Stopping before switching environment...");
            _processManager.KillProcesses(pids, force: false);

            // Verify processes are stopped
            var remainingPids = _processManager.FindGameProcesses();
            if (remainingPids.Count > 0)
            {
                _processManager.KillProcesses(remainingPids, force: true);
            }

            Console.WriteLine("Game stopped.");
        }

        // Perform the swap
        Console.WriteLine();
        Console.WriteLine($"Switching from {current} to {target}...");

        var sourcePath = target == "debug" ? debugDllPath : releaseDllPath;

        try
        {
            _fileSystem.CopyFile(sourcePath, activeDllPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: Failed to copy DLL: {ex.Message}");
            return 1;
        }

        Console.WriteLine();
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine($"✓ Successfully switched from {current} to {target}");
        Console.WriteLine("".PadRight(60, '='));
        return 0;
    }

    private string? DetectCurrentEnvironment(string activeDllPath, string debugDllPath, string releaseDllPath)
    {
        try
        {
            var activeBytes = _fileSystem.ReadAllBytes(activeDllPath);
            var debugBytes = _fileSystem.ReadAllBytes(debugDllPath);
            var releaseBytes = _fileSystem.ReadAllBytes(releaseDllPath);

            if (activeBytes.SequenceEqual(debugBytes))
            {
                return "debug";
            }

            if (activeBytes.SequenceEqual(releaseBytes))
            {
                return "release";
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool HasArg(string[] args, string argName)
    {
        return args.Contains(argName);
    }

    private static void PrintUsage()
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("Usage: dotnet playwright switch-environment <debug|release>");
        Console.Error.WriteLine("   or: dotnet playwright switch-environment --status");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Options:");
        Console.Error.WriteLine("  --status    Show current environment without switching");
        Console.Error.WriteLine();
    }
}
