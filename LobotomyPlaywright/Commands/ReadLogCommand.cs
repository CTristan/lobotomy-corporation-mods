// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Linq;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to read BepInEx log files from the game directory.
/// </summary>
internal class ReadLogCommand
{
    private const string DefaultLogFile = "LogOutput.log";
    private const string BepInExLogDir = "BepInEx";

    private readonly IConfigManager _configManager;
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of ReadLogCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    public ReadLogCommand(IConfigManager configManager, IFileSystem fileSystem)
    {
        _configManager = configManager;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Initializes a new instance of ReadLogCommand class with default implementations.
    /// </summary>
    public ReadLogCommand()
        : this(new ConfigManager(new FileSystem()), new FileSystem())
    {
    }

    /// <summary>
    /// Runs the read-log command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        var logFile = GetArgValue(args, "--file") ?? DefaultLogFile;
        var tail = GetIntArgValue(args, "--tail") ?? 0;
        var filter = GetArgValue(args, "--filter");
        var listFiles = HasArg(args, "--list");

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

        var gamePath = config.GamePath;

        if (!_fileSystem.DirectoryExists(gamePath))
        {
            Console.Error.WriteLine($"ERROR: Game path does not exist: {gamePath}");
            Console.Error.WriteLine("The volume may not be mounted. Run 'dotnet playwright find-game' to reconfigure.");
            return 1;
        }

        var logDir = Path.Combine(gamePath, BepInExLogDir);

        if (!_fileSystem.DirectoryExists(logDir))
        {
            Console.Error.WriteLine($"ERROR: BepInEx log directory does not exist: {logDir}");
            Console.Error.WriteLine("BepInX may not be installed properly.");
            return 1;
        }

        // List available log files
        if (listFiles)
        {
            ListLogFiles(logDir);
            return 0;
        }

        var logPath = Path.Combine(logDir, logFile);

        if (!_fileSystem.FileExists(logPath))
        {
            Console.Error.WriteLine($"ERROR: Log file does not exist: {logPath}");
            Console.Error.WriteLine("Use --list to see available log files.");
            return 1;
        }

        // Read and display the log
        DisplayLogContent(logPath, tail, filter);

        return 0;
    }

    private void ListLogFiles(string logDir)
    {
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine("Available Log Files");
        Console.WriteLine("".PadRight(60, '='));

        try
        {
            var files = _fileSystem.GetFiles(logDir, "*.log")
                .OrderByDescending(f => _fileSystem.GetLastWriteTime(f))
                .ToArray();

            if (files.Length == 0)
            {
                Console.WriteLine("No log files found.");
                return;
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var lastModified = _fileSystem.GetLastWriteTime(file);
                var size = _fileSystem.GetFileSize(file);

                Console.WriteLine($"  {fileName}");
                Console.WriteLine($"    Size: {size:N0} bytes");
                Console.WriteLine($"    Modified: {lastModified:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: Failed to list log files: {ex.Message}");
        }
    }

    private void DisplayLogContent(string logPath, int tail, string? filter)
    {
        var separator = "=".PadRight(60, '=');

        try
        {
            var content = _fileSystem.ReadAllText(logPath);

            if (content == null)
            {
                Console.Error.WriteLine("ERROR: Failed to read log file (null content returned).");
                return;
            }

            var lines = content.Split('\n');

            if (string.IsNullOrEmpty(filter))
            {
                // No filter, just display (with tail if specified)
                var linesToDisplay = tail > 0 ? lines.TakeLast(tail) : lines;
                Console.WriteLine(separator);
                Console.WriteLine($"Log: {logPath}");
                if (tail > 0)
                {
                    Console.WriteLine($"Showing last {tail} lines of {lines.Length} total lines");
                }
                else
                {
                    Console.WriteLine($"Total lines: {lines.Length}");
                }
                Console.WriteLine(separator);
                Console.WriteLine();

                foreach (var line in linesToDisplay)
                {
                    Console.WriteLine(line);
                }
            }
            else
            {
                // Filter lines
                var filteredLines = lines.Where(l => l.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();

                Console.WriteLine(separator);
                Console.WriteLine($"Log: {logPath}");
                Console.WriteLine($"Filter: {filter}");
                Console.WriteLine($"Found {filteredLines.Length} matching lines out of {lines.Length} total");
                Console.WriteLine(separator);
                Console.WriteLine();

                foreach (var line in filteredLines)
                {
                    Console.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: Failed to read log file: {ex.Message}");
        }
    }

    private static string? GetArgValue(string[] args, string argName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == argName && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static int? GetIntArgValue(string[] args, string argName)
    {
        var value = GetArgValue(args, argName);
        if (value == null || !int.TryParse(value, out var result))
        {
            return null;
        }

        return result;
    }

    private static bool HasArg(string[] args, string argName)
    {
        foreach (var arg in args)
        {
            if (arg == argName)
            {
                return true;
            }
        }

        return false;
    }
}
