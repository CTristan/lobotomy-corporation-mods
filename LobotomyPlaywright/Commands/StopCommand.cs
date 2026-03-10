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
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to stop the game with graceful TCP shutdown or force-kill.
/// </summary>
public class StopCommand
{
    private readonly IConfigManager _configManager;
    private readonly ProcessManager _processManager;

    /// <summary>
    /// Initializes a new instance of StopCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="processManager">The process manager.</param>
    public StopCommand(IConfigManager configManager, ProcessManager processManager)
    {
        _configManager = configManager;
        _processManager = processManager;
    }

    /// <summary>
    /// Initializes a new instance of StopCommand class with default implementations.
    /// </summary>
    public StopCommand()
        : this(new ConfigManager(new FileSystem()), new ProcessManager())
    {
    }

    /// <summary>
    /// Runs the stop command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        var host = GetArgValue(args, "--host") ?? "localhost";
        var portArg = GetArgValue(args, "--port");
        var timeoutArg = GetArgValue(args, "--timeout");
        var force = HasArg(args, "--force");
        var waitFlag = HasArg(args, "--wait");

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

        var port = portArg != null ? int.Parse(portArg) : config.TcpPort;
        var timeout = timeoutArg != null ? double.Parse(timeoutArg) : config.ShutdownTimeoutSeconds;

        // Find game processes
        var pids = _processManager.FindGameProcesses();

        if (pids.Count == 0)
        {
            Console.WriteLine("Game is not running");
            return 0;
        }

        Console.WriteLine($"Found {pids.Count} game process(es): {string.Join(", ", pids)}");

        // Stop the game
        Console.WriteLine();
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine("Stopping Lobotomy Corporation");
        Console.WriteLine("".PadRight(60, '='));

        var success = false;

        if (!force)
        {
            // Try graceful shutdown first
            if (IsTcpPortOpen(host, port))
            {
                success = TryGracefulShutdown(host, port, timeout);
            }
            else
            {
                Console.WriteLine("TCP server not available, proceeding to force kill");
            }
        }

        // Force kill if graceful failed or was skipped
        if (!success)
        {
            success = TryForceKill(pids);
        }

        // Wait for processes to exit if requested
        if (waitFlag)
        {
            Console.WriteLine();
            Console.WriteLine("Waiting for processes to exit...");

            for (var i = 0; i < 10; i++)
            {
                var remaining = _processManager.FindGameProcesses();
                if (remaining.Count == 0)
                {
                    Console.WriteLine("  ✓ All processes exited");
                    break;
                }

                Thread.Sleep(500);
            }

            var finalRemaining = _processManager.FindGameProcesses();
            if (finalRemaining.Count > 0)
            {
                Console.WriteLine($"  ⚠ Processes still running: {string.Join(", ", finalRemaining)}");
            }
        }

        // Final status
        var finalPids = _processManager.FindGameProcesses();

        Console.WriteLine();
        Console.WriteLine("".PadRight(60, '='));

        if (success && finalPids.Count == 0)
        {
            Console.WriteLine("✓ Game stopped successfully");
            Console.WriteLine("".PadRight(60, '='));
            return 0;
        }
        else
        {
            Console.WriteLine("✗ Failed to stop game completely");
            if (finalPids.Count > 0)
            {
                Console.WriteLine($"  Remaining processes: {string.Join(", ", finalPids)}");
            }

            Console.WriteLine("".PadRight(60, '='));
            return 1;
        }
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
        catch (Exception ex) when (ex is SocketException || ex is InvalidOperationException)
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

    private bool TryForceKill(List<int> pids)
    {
        Console.WriteLine($"Force killing processes: {string.Join(", ", pids)}");

        var success = _processManager.KillProcesses(pids, force: false);

        if (!success)
        {
            // Try SIGKILL
            success = _processManager.KillProcesses(pids, force: true);
        }

        if (success)
        {
            Console.WriteLine("  ✓ All processes force-killed");
        }
        else
        {
            Console.WriteLine("  ✗ Failed to kill all processes");
        }

        return success;
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
        public static System.Collections.Generic.List<int> FindGameProcessesStatic()
        {
            var pids = new System.Collections.Generic.List<int>();
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
                    process.Start();
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
