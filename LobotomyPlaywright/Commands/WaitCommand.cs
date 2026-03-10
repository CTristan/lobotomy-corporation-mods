// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Network;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to wait for game events.
/// </summary>
public class WaitCommand
{
    private readonly IConfigManager _configManager;
    private readonly Func<ITcpClient> _tcpClientFactory;

    /// <summary>
    /// Initializes a new instance of WaitCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="tcpClientFactory">Factory for creating TCP clients.</param>
    public WaitCommand(IConfigManager configManager, Func<ITcpClient> tcpClientFactory)
    {
        _configManager = configManager;
        _tcpClientFactory = tcpClientFactory;
    }

    /// <summary>
    /// Initializes a new instance of WaitCommand class with default implementations.
    /// </summary>
    public WaitCommand()
        : this(new ConfigManager(new FileSystem()), () => new PlaywrightTcpClient())
    {
    }

    /// <summary>
    /// Runs the wait command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return 1;
        }

        var subcommand = args[0].ToLower();

        if (subcommand != "event")
        {
            Console.Error.WriteLine($"Unknown subcommand: {subcommand}");
            Console.Error.WriteLine("Only 'event' subcommand is supported. Condition-based waiting has been removed for security.");
            return 1;
        }

        // Extract event names (everything between "event" and next --arg)
        var eventNames = new List<string>();
        var i = 1;
        while (i < args.Length && !args[i].StartsWith("--"))
        {
            eventNames.Add(args[i]);
            i++;
        }

        if (eventNames.Count == 0)
        {
            Console.Error.WriteLine("ERROR: No event names specified");
            return 1;
        }

        var host = GetArgValue(args, "--host") ?? "localhost";
        var portArg = GetArgValue(args, "--port");
        var timeoutArg = GetArgValue(args, "--timeout");
        var jsonOutput = HasArg(args, "--json");

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
        var timeout = timeoutArg != null ? double.Parse(timeoutArg) : 60.0;

        try
        {
            using var client = _tcpClientFactory();
            client.Connect(host, port);

            // Subscribe to events
            if (!client.Subscribe(eventNames.ToArray()))
            {
                Console.Error.WriteLine("ERROR: Failed to subscribe to events");
                return 1;
            }

            Console.WriteLine($"Waiting for events: {string.Join(", ", eventNames)}");
            Console.WriteLine($"Timeout: {timeout}s");
            Console.WriteLine();

            // Wait for first matching event
            var eventData = client.WaitForEvent(timeout);

            if (eventData == null)
            {
                Console.Error.WriteLine($"ERROR: Timeout waiting for events: {string.Join(", ", eventNames)}");
                return 1;
            }

            // Display event
            if (jsonOutput)
            {
                Console.WriteLine(JsonSerializer.Serialize(eventData, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                PrintEventHumanReadable(eventData);
            }

            return 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Net.Sockets.SocketException)
        {
            Console.Error.WriteLine($"Connection error: {ex.Message}");
            Console.Error.WriteLine("Ensure Lobotomy Corporation is running with LobotomyPlaywright plugin.");
            return 1;
        }
    }

    private static void PrintEventHumanReadable(Dictionary<string, object> eventData)
    {
        var eventName = eventData.TryGetValue("event", out var e) ? e?.ToString() ?? "Unknown" : "Unknown";
        var timestamp = eventData.TryGetValue("timestamp", out var t) ? t?.ToString() ?? "" : "";
        var data = eventData.TryGetValue("data", out var d) && d is Dictionary<string, object> dataDict
            ? dataDict
            : new Dictionary<string, object>();

        Console.WriteLine($"Event: {eventName}");
        if (!string.IsNullOrEmpty(timestamp))
        {
            Console.WriteLine($"Time: {timestamp}");
        }

        if (data.Count > 0)
        {
            Console.WriteLine("Data:");
            foreach (var kvp in data)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage: dotnet playwright wait event <event-names...> [options]");
        Console.WriteLine();
        Console.WriteLine("Subcommands:");
        Console.WriteLine("  event               Wait for specific game events (supported only)");
        Console.WriteLine();
        Console.WriteLine("Event Names:");
        Console.WriteLine("  OnAgentDead          Agent died");
        Console.WriteLine("  OnAgentPanic         Agent panicked");
        Console.WriteLine("  OnWorkStart          Work started on a creature");
        Console.WriteLine("  OnWorkCoolTimeEnd    Work cooldown ended");
        Console.WriteLine("  OnCreatureSuppressed Creature suppressed");
        Console.WriteLine("  OnEscape             Creature escaped");
        Console.WriteLine("  OnOrdealStarted      Ordeal started");
        Console.WriteLine("  OnNextDay            Day ended, next day started");
        Console.WriteLine("  OnStageStart         Stage started");
        Console.WriteLine("  OnStageEnd           Stage ended");
        Console.WriteLine("  OnGetEGOgift         E.G.O. gift obtained");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --host HOST         Connect to specific host (default: localhost)");
        Console.WriteLine("  --port PORT         Connect to specific port (default: from config)");
        Console.WriteLine("  --timeout SECONDS   Maximum time to wait (default: 60)");
        Console.WriteLine("  --json              Output event data as JSON");
        Console.WriteLine();
        Console.WriteLine("Note: Condition-based waiting has been removed for security reasons.");
        Console.WriteLine("Only event-based waiting is supported.");
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
