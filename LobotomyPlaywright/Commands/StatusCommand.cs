// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Game status enumeration.
    /// </summary>
    internal enum GameStatus
    {
        Stopped,
        Starting,
        Ready,
        Unresponsive
    }

    /// <summary>
    /// Command to check the status of the game and TCP server.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    public class StatusCommand(IConfigManager configManager)
    {
        private readonly IConfigManager _configManager = configManager;

        /// <summary>
        /// Initializes a new instance of StatusCommand class with default implementations.
        /// </summary>
        public StatusCommand()
            : this(new ConfigManager(new FileSystem()))
        {
        }

        /// <summary>
        /// Runs the status command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            var host = GetArgValue(args, "--host") ?? "localhost";
            var portArg = GetArgValue(args, "--port");
            var jsonOutput = HasArg(args, "--json");
            var exitCodeMode = HasArg(args, "--exit-code");
            var waitForArg = GetArgValue(args, "--wait-for");
            var timeoutArg = GetArgValue(args, "--timeout");
            var pollArg = GetArgValue(args, "--poll");

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

            var port = portArg != null ? int.Parse(portArg) : config.TcpPort;

            // Wait for specific status
            if (waitForArg != null)
            {
                var targetStatus = ParseGameStatus(waitForArg);
                var timeout = timeoutArg != null ? int.Parse(timeoutArg) : 60;
                var poll = pollArg != null ? double.Parse(pollArg) : 1.0;

                var startTime = DateTime.UtcNow;

                while ((DateTime.UtcNow - startTime).TotalSeconds < timeout)
                {
                    var (status, _, _) = GetGameStatus(host, port);

                    if (status == targetStatus)
                    {
                        break;
                    }

                    Thread.Sleep((int)(poll * 1000));
                }
            }

            // Get current status
            var (currentStatus, pid, gameState) = GetGameStatus(host, port);

            // Output
            if (jsonOutput)
            {
                var output = new Dictionary<string, object?>
                {
                    { "status", currentStatus.ToString().ToUpper(System.Globalization.CultureInfo.InvariantCulture) },
                    { "pid", pid },
                    { "host", host },
                    { "port", port }
                };

                if (gameState != null)
                {
                    output["gameState"] = gameState;
                }

                Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                PrintFormattedStatus(currentStatus, pid, host, port, gameState);
            }

            // Exit code
            if (exitCodeMode && currentStatus != GameStatus.Ready)
            {
                return 1;
            }

            return 0;
        }

        private static (GameStatus Status, int? Pid, Dictionary<string, object>? GameState) GetGameStatus(string host, int port)
        {
            var pids = ProcessManagerStatic.FindGameProcessesStatic();

            if (pids.Count == 0)
            {
                return (GameStatus.Stopped, null, null);
            }

            // Game process is running
            if (!IsTcpPortOpen(host, port))
            {
                return (GameStatus.Starting, pids[0], null);
            }

            // TCP port is open, check if plugin is responsive
            if (!IsPluginResponsive(host, port))
            {
                return (GameStatus.Unresponsive, pids[0], null);
            }

            // Plugin is responsive, get game state
            var gameState = QueryGameState(host, port);
            return (GameStatus.Ready, pids[0], gameState);
        }

        private static void PrintFormattedStatus(GameStatus status, int? pid, string host, int port, Dictionary<string, object>? gameState)
        {
            Console.WriteLine("".PadRight(60, '='));

            // Status
            var statusEmoji = status switch
            {
                GameStatus.Stopped => "⏹",
                GameStatus.Starting => "⏳",
                GameStatus.Ready => "✅",
                GameStatus.Unresponsive => "⚠️",
                _ => "?"
            };

            Console.WriteLine($"Status: {statusEmoji} {status.ToString().ToUpper()}");
            Console.WriteLine("".PadRight(60, '='));

            // Process ID
            if (pid.HasValue)
            {
                Console.WriteLine($"Process ID: {pid.Value}");
            }
            else
            {
                Console.WriteLine("Process: Not running");
            }

            // TCP Server
            if (status is GameStatus.Starting or GameStatus.Ready or GameStatus.Unresponsive)
            {
                Console.WriteLine($"TCP Server: {host}:{port}");

                if (status == GameStatus.Starting)
                {
                    Console.WriteLine("  → Waiting for plugin to load...");
                }
                else if (status == GameStatus.Unresponsive)
                {
                    Console.WriteLine("  ⚠ Port open but plugin not responding");
                }
                else if (status == GameStatus.Ready)
                {
                    Console.WriteLine("  → Connected and responsive");
                }
            }

            // Game State (only if ready)
            if (status == GameStatus.Ready && gameState != null)
            {
                Console.WriteLine();
                Console.WriteLine("Game State:");
                Console.WriteLine($"  Day: {GetValue(gameState, "day", 0)}");
                Console.WriteLine($"  Phase: {GetValue(gameState, "gameState", "N/A")}");
                Console.WriteLine($"  Speed: {GetValue(gameState, "gameSpeed", 1)}x");
                Console.WriteLine($"  Energy: {GetValue(gameState, "energy", 0.0):F1}/{GetValue(gameState, "energyQuota", 0.0):F1}");
                Console.WriteLine($"  Emergency: {GetValue(gameState, "emergencyLevel", "NORMAL")}");
                Console.WriteLine($"  Management Started: {GetValue(gameState, "managementStarted", false)}");
                Console.WriteLine($"  Paused: {GetValue(gameState, "isPaused", false)}");
            }

            Console.WriteLine("".PadRight(60, '='));
        }

        private static GameStatus ParseGameStatus(string value)
        {
            return value.ToUpper() switch
            {
                "STOPPED" => GameStatus.Stopped,
                "STARTING" => GameStatus.Starting,
                "READY" => GameStatus.Ready,
                "UNRESPONSIVE" => GameStatus.Unresponsive,
                _ => throw new ArgumentException($"Invalid game status: {value}")
            };
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
                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "pgrep",
                                Arguments = "-f LobotomyCorp.exe",
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
                    // Ignore errors
                }

                return pids;
            }
        }
    }
}
