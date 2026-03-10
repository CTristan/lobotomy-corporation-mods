// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Command to find and configure the game installation path.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of FindGameCommand class.
    /// </remarks>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="pathFinder">The game path finder.</param>
    public class FindGameCommand(IConfigManager configManager, IFileSystem fileSystem, GamePathFinder pathFinder)
    {
        private readonly IConfigManager _configManager = configManager;
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly GamePathFinder _pathFinder = pathFinder;

        /// <summary>
        /// Initializes a new instance of FindGameCommand class with default implementations.
        /// </summary>
        public FindGameCommand()
            : this(
                new ConfigManager(new FileSystem()),
                new FileSystem(),
                new GamePathFinder(new FileSystem(), Console.WriteLine))
        {
        }

        /// <summary>
        /// Runs the find-game command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            var path = GetArgValue(args, "--path");
            var bottle = GetArgValue(args, "--bottle");
            var checkMode = HasArg(args, "--check");
            var verbose = HasArg(args, "--verbose");

            if (verbose)
            {
                Console.WriteLine("Verbose mode enabled");
            }

            // Check mode: validate existing config
            if (checkMode)
            {
                return ValidateExistingConfig(verbose);
            }

            // Determine game path and bottle
            string gamePath;
            string? bottleName;

            if (path != null)
            {
                gamePath = path;
                bottleName = bottle;

                if (!_fileSystem.DirectoryExists(gamePath))
                {
                    Console.Error.WriteLine($"ERROR: Path does not exist: {gamePath}");
                    return 1;
                }

                if (!_pathFinder.IsValidGamePath(gamePath))
                {
                    Console.Error.WriteLine($"ERROR: Path is not a valid Lobotomy Corporation installation: {gamePath}");
                    return 1;
                }

                Console.WriteLine($"Using manual game path: {gamePath}");
            }
            else
            {
                var result = _pathFinder.FindGamePath();
                if (result == null)
                {
                    Console.Error.WriteLine("ERROR: Could not auto-detect game path");
                    Console.Error.WriteLine("Use --path to manually specify the game installation directory");
                    return 1;
                }

                gamePath = result.Value.GamePath;
                bottleName = result.Value.BottleName;
            }

            // Check BepInEx installation
            Console.WriteLine("Validating BepInEx installation...");
            if (_pathFinder.IsBepInExInstalled(gamePath))
            {
                Console.WriteLine("BepInEx installation: OK");
            }
            else
            {
                Console.WriteLine("WARNING: BepInEx does not appear to be installed");
                Console.WriteLine("  Expected: BepInEx/core/BepInEx.dll and doorstop_config.ini");
                Console.WriteLine("  The plugin will not work without BepInEx");
            }

            // Create config
            Console.WriteLine("Creating configuration...");
            var config = new Config
            {
                GamePath = gamePath,
                CrossoverBottle = bottleName,
                TcpPort = 8484,
                LaunchTimeoutSeconds = 120,
                ShutdownTimeoutSeconds = 10,
            };

            _configManager.Save(config);

            // Summary
            Console.WriteLine();
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Configuration Summary:");
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine($"Game Path: {gamePath}");
            Console.WriteLine($"CrossOver Bottle: {bottleName ?? "N/A"}");
            Console.WriteLine($"TCP Port: {config.TcpPort}");
            Console.WriteLine($"Launch Timeout: {config.LaunchTimeoutSeconds}s");
            Console.WriteLine($"Shutdown Timeout: {config.ShutdownTimeoutSeconds}s");
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine();
            Console.WriteLine("You can now use:");
            Console.WriteLine("  dotnet playwright deploy   # Build and deploy the plugin");
            Console.WriteLine("  dotnet playwright launch   # Launch the game");
            Console.WriteLine("  dotnet playwright status   # Check game status");

            return 0;
        }

        private int ValidateExistingConfig(bool verbose)
        {
            try
            {
                var config = _configManager.Load();

                Console.WriteLine("Current configuration:");
                Console.WriteLine($"  Game Path: {config.GamePath}");
                Console.WriteLine($"  CrossOver Bottle: {config.CrossoverBottle ?? "N/A"}");
                Console.WriteLine($"  TCP Port: {config.TcpPort}");

                if (!_fileSystem.DirectoryExists(config.GamePath))
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("ERROR: Game path does not exist");
                    return 1;
                }

                if (!_pathFinder.IsValidGamePath(config.GamePath))
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("ERROR: Game path is not a valid Lobotomy Corporation installation");
                    return 1;
                }

                if (_pathFinder.IsBepInExInstalled(config.GamePath))
                {
                    Console.WriteLine();
                    Console.WriteLine("BepInEx installation: OK");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("WARNING: BepInX does not appear to be installed at this path");
                    Console.WriteLine("  Expected: BepInEx/core/BepInEx.dll and doorstop_config.ini");
                }

                Console.WriteLine();
                Console.WriteLine("Configuration is valid");
                return 0;
            }
            catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
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
}
