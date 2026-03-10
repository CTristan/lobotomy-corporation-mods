// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Command to launch the game and wait for TCP readiness.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of LaunchCommand class.
    /// </remarks>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="processManager">The process manager.</param>
    public class LaunchCommand(IConfigManager configManager, IFileSystem fileSystem, ProcessManager processManager)
    {
        private readonly IConfigManager _configManager = configManager;
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly ProcessManager _processManager = processManager;

        /// <summary>
        /// Initializes a new instance of LaunchCommand class with default implementations.
        /// </summary>
        public LaunchCommand()
            : this(new ConfigManager(new FileSystem()), new FileSystem(), new ProcessManager())
        {
        }

        /// <summary>
        /// Runs the launch command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            var host = GetArgValue(args, "--host") ?? "localhost";
            var portArg = GetArgValue(args, "--port");
            var timeoutArg = GetArgValue(args, "--timeout");
            var noWait = HasArg(args, "--no-wait");
            var force = HasArg(args, "--force");

            // Load configuration
            Config config;
            try
            {
                config = _configManager.Load();
            }
            catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }

            var gamePath = config.GamePath;
            var bottleName = config.CrossoverBottle;
            var port = portArg != null ? int.Parse(portArg) : config.TcpPort;
            var timeout = timeoutArg != null ? int.Parse(timeoutArg) : config.LaunchTimeoutSeconds;

            if (!_fileSystem.DirectoryExists(gamePath))
            {
                Console.Error.WriteLine($"ERROR: Game path does not exist: {gamePath}");
                Console.Error.WriteLine("The volume may not be mounted. Run 'dotnet playwright find-game' to reconfigure.");
                return 1;
            }

            // Check if game is already running
            var existingPids = _processManager.FindGameProcesses();
            if (existingPids.Count > 0)
            {
                Console.WriteLine($"Game is already running (PID: {string.Join(", ", existingPids)}). Stopping it first...");

                var stopped = false;
                if (!force && IsTcpPortOpen(host, port))
                {
                    stopped = TryGracefulShutdown(host, port, config.ShutdownTimeoutSeconds);
                }

                if (!stopped)
                {
                    if (!force)
                    {
                        Console.WriteLine("Graceful shutdown failed or not available, force killing...");
                    }
                    else
                    {
                        Console.WriteLine("Force flag set, killing processes immediately...");
                    }

                    _ = _processManager.KillProcesses(existingPids, force: true);
                }

                // Wait for processes to fully exit
                var stopWaitStartTime = DateTime.UtcNow;
                var stopSuccess = false;
                while ((DateTime.UtcNow - stopWaitStartTime).TotalSeconds < 10)
                {
                    if (_processManager.FindGameProcesses().Count == 0)
                    {
                        stopSuccess = true;
                        break;
                    }
                    Thread.Sleep(500);
                }

                if (!stopSuccess)
                {
                    Console.Error.WriteLine("ERROR: Failed to stop existing game process. Cannot launch new instance.");
                    return 1;
                }

                Console.WriteLine("Existing game instance stopped.");
                Thread.Sleep(1000); // Small buffer
            }

            // Launch the game
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Launching Lobotomy Corporation");
            Console.WriteLine("".PadRight(60, '='));

            try
            {
                LaunchGame(gamePath, bottleName);
            }
            catch (Exception ex) when (ex is FileNotFoundException or NotImplementedException)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"ERROR: Failed to launch game: {ex.Message}");
                return 1;
            }

            // Wait for readiness
            if (!noWait)
            {
                var (success, gameState) = WaitPluginReady(host, port, timeout);

                if (!success)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("ERROR: Game did not become ready within timeout");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Possible issues:");
                    Console.Error.WriteLine("  - BepInEx may not be installed correctly");
                    Console.Error.WriteLine("  - Plugin may not be loaded (check BepInEx log)");
                    Console.Error.WriteLine("  - Game may have crashed (check for error dialogs)");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Suggestions:");
                    Console.Error.WriteLine("  - Run 'dotnet playwright status' to check game status");
                    Console.Error.WriteLine("  - Check <game_path>/BepInEx/LogOutput.log for errors");
                    Console.Error.WriteLine("  - Try launching the game manually to see any error messages");
                    return 1;
                }

                // Print game state
                if (gameState != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("".PadRight(60, '='));
                    Console.WriteLine("Game State");
                    Console.WriteLine("".PadRight(60, '='));
                    PrintGameState(gameState);
                    Console.WriteLine("".PadRight(60, '='));
                }

                Console.WriteLine();
                Console.WriteLine("Game is ready!");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  dotnet playwright query agents    # Query game state");
                Console.WriteLine("  dotnet playwright status         # Check status");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Game launched. Run 'dotnet playwright status' to check readiness.");
            }

            return 0;
        }

        private static bool TryGracefulShutdown(string host, int port, double timeout)
        {
            Console.WriteLine("Attempting graceful TCP shutdown...");

            try
            {
                using var client = new TcpClient();
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;
                client.Connect(host, port);

                var stream = client.GetStream();
                var command = new { type = "command", action = "shutdown", @params = new { } };
                var message = JsonSerializer.Serialize(command) + "\n";
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                // Wait for acknowledgment
                try
                {
                    var buffer = new byte[4096];
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        var responseJson = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
                        if (response != null && response.TryGetValue("status", out var status) && status?.ToString() == "ok")
                        {
                            Console.WriteLine("  Shutdown command acknowledged");
                        }
                    }
                }
                catch
                {
                    // Ignore acknowledgment errors
                }
            }
            catch (Exception ex) when (ex is SocketException or InvalidOperationException)
            {
                Console.WriteLine($"  ⚠ TCP connection failed: {ex.Message}");
                Console.WriteLine("  Proceeding to force kill");
                return false;
            }

            // Wait for process to exit
            Console.WriteLine($"  Waiting for process to exit (timeout: {timeout}s)...");
            var startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - startTime).TotalSeconds < timeout)
            {
                var pids = ProcessManagerStatic.FindGameProcessesStatic();
                if (pids.Count == 0)
                {
                    Console.WriteLine("  ✓ Game stopped gracefully");
                    return true;
                }

                Thread.Sleep(500);
            }

            Console.WriteLine("  ⚠ Timeout exceeded, proceeding to force kill");
            return false;
        }

        private static void LaunchGame(string gamePath, string? bottleName)
        {
            var gameExe = Path.Combine(gamePath, "LobotomyCorp.exe");

            if (!File.Exists(gameExe))
            {
                throw new FileNotFoundException($"Game executable not found: {gameExe}");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: Use CrossOver
                var cxstartPath = FindCxstartPath() ?? throw new FileNotFoundException(
                        "CrossOver cxstart not found.\n" +
                        "Install CrossOver from https://www.codeweavers.com/crossover\n" +
                        "or set CROSSOVER_APP environment variable to your CrossOver installation.");
                var cmd = new ProcessStartInfo
                {
                    FileName = cxstartPath,
                    Arguments = $"--bottle {bottleName ?? "LobotomyCorp"} --workdir \"{gamePath}\" --dll winhttp=n,b \"{gameExe}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                _ = Process.Start(cmd);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new NotImplementedException("Windows launch not yet implemented");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotImplementedException("Linux launch (via Proton) not yet implemented");
            }
            else
            {
                throw new NotImplementedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
            }
        }

        private static string? FindCxstartPath()
        {
            var cxstartPath = "/Applications/CrossOver.app/Contents/SharedSupport/CrossOver/bin/cxstart";

            if (File.Exists(cxstartPath))
            {
                return cxstartPath;
            }

            var appsPath = "/Applications";
            if (Directory.Exists(appsPath))
            {
                foreach (var item in Directory.GetDirectories(appsPath))
                {
                    var itemName = Path.GetFileName(item);
                    if (itemName?.StartsWith("CrossOver") == true)
                    {
                        var potentialPath = Path.Combine(item, "Contents", "SharedSupport", "CrossOver", "bin", "cxstart");
                        if (File.Exists(potentialPath))
                        {
                            return potentialPath;
                        }
                    }
                }
            }

            return null;
        }

        private static (bool Success, Dictionary<string, object>? GameState) WaitPluginReady(string host, int port, int timeout)
        {
            var startTime = DateTime.UtcNow;
            var elapsed = 0;

            Console.WriteLine();
            Console.WriteLine($"Waiting for game to be ready (timeout: {timeout}s)...");
            Console.WriteLine();

            while (elapsed < timeout)
            {
                if (IsTcpPortOpen(host, port))
                {
                    // Port is open, check if plugin is responsive
                    if (IsPluginResponsive(host, port))
                    {
                        Console.WriteLine($"Game is ready! ({elapsed}s elapsed)");

                        // Get game state
                        var gameState = QueryGameState(host, port);
                        return (true, gameState);
                    }

                    Console.WriteLine($"[{elapsed}s] Port open but plugin not responding yet...");
                }

                Console.Write($"[{elapsed}s] Waiting for TCP server...\r");
                Thread.Sleep(2000);
                elapsed = (int)(DateTime.UtcNow - startTime).TotalSeconds;
            }

            Console.WriteLine();
            return (false, null);
        }

        private static bool IsTcpPortOpen(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                client.Connect(host, port);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPluginResponsive(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                client.ReceiveTimeout = 1000;
                client.SendTimeout = 1000;
                client.Connect(host, port);

                var stream = client.GetStream();
                var query = new { id = "ping", type = "query", target = "game", @params = new { } };
                var message = JsonSerializer.Serialize(query) + "\n";
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                var buffer = new byte[4096];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    var responseJson = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
                    return response != null && response.TryGetValue("status", out var status) && status?.ToString() == "ok";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, object>? QueryGameState(string host, int port)
        {
            try
            {
                using var client = new TcpClient();
                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;
                client.Connect(host, port);

                var stream = client.GetStream();
                var query = new { id = "status", type = "query", target = "game", @params = new { } };
                var message = JsonSerializer.Serialize(query) + "\n";
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);

                var buffer = new byte[8192];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    var responseJson = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
                    if (response != null && response.TryGetValue("data", out var data) && data is Dictionary<string, object> dataDict)
                    {
                        return dataDict;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static void PrintGameState(Dictionary<string, object> gameState)
        {
            Console.WriteLine($"Day: {GetValue(gameState, "day", 0)}");
            Console.WriteLine($"Phase: {GetValue(gameState, "gameState", "N/A")}");
            Console.WriteLine($"Speed: {GetValue(gameState, "gameSpeed", 1)}x");
            Console.WriteLine($"Energy: {GetValue(gameState, "energy", 0.0):F1}/{GetValue(gameState, "energyQuota", 0.0):F1}");
            Console.WriteLine($"Emergency: {GetValue(gameState, "emergencyLevel", "NORMAL")}");
            Console.WriteLine($"Management Started: {GetValue(gameState, "managementStarted", false)}");
            Console.WriteLine($"Paused: {GetValue(gameState, "isPaused", false)}");
        }

        private static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue)
        {
            if (dict.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
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

        private static class ProcessManagerStatic
        {
            public static List<int> FindGameProcessesStatic()
            {
                var pids = new List<int>();
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        // Use ps command instead of pgrep for better reliability with Wine/CrossOver
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "/bin/sh",
                                Arguments = "-c \"ps aux | grep -i 'LobotomyCorp.exe' | grep -v grep | awk '{print $2}'\"",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        _ = process.Start();
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (int.TryParse(line.Trim(), out var pid))
                            {
                                pids.Add(pid);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore errors - if we can't detect the process, assume it's not running
                    // This is safer than crashing the launch command
                }

                return pids;
            }
        }
    }
}
