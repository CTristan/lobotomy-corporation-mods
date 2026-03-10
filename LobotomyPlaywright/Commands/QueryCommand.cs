// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Network;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Command to query game state.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of QueryCommand class.
    /// </remarks>
    /// <param name="configManager">The config manager.</param>
    /// <param name="tcpClientFactory">Factory for creating TCP clients.</param>
    public class QueryCommand(IConfigManager configManager, Func<ITcpClient> tcpClientFactory)
    {
        private readonly IConfigManager _configManager = configManager;
        private readonly Func<ITcpClient> _tcpClientFactory = tcpClientFactory;

        /// <summary>
        /// Initializes a new instance of QueryCommand class with default implementations.
        /// </summary>
        public QueryCommand()
            : this(new ConfigManager(new FileSystem()), () => new PlaywrightTcpClient())
        {
        }

        /// <summary>
        /// Runs the query command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var target = args[0].ToLower();
            var idArg = args.Length > 1 ? args[1] : null;
            var host = GetArgValue(args, "--host") ?? "localhost";
            var portArg = GetArgValue(args, "--port");
            var jsonOutput = HasArg(args, "--json");

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

            try
            {
                using var client = _tcpClientFactory();
                client.Connect(host, port);

                return ExecuteQuery(client, target, idArg, jsonOutput, args);
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Net.Sockets.SocketException)
            {
                Console.Error.WriteLine($"Connection error: {ex.Message}");
                Console.Error.WriteLine("Ensure Lobotomy Corporation is running with LobotomyPlaywright plugin.");
                return 1;
            }
        }

        private int ExecuteQuery(ITcpClient client, string target, string? idArg, bool jsonOutput, string[] args)
        {
            return target switch
            {
                "agents" or "agent" => QueryAgents(client, idArg, jsonOutput),
                "creatures" or "creature" or "abnormalities" or "abnormality" => QueryCreatures(client, idArg, jsonOutput),
                "game" or "status" => QueryGame(client, jsonOutput),
                "departments" or "department" or "sefira" or "sefiras" => QueryDepartments(client, jsonOutput),
                "ui" => QueryUi(client, jsonOutput, args),
                _ => PrintUnknownTargetError(target)
            };
        }

        private static int QueryAgents(ITcpClient client, string? idArg, bool jsonOutput)
        {
            if (idArg != null)
            {
                var agentParams = new Dictionary<string, object> { { "id", int.Parse(idArg) } };
                var agentData = client.Query("agents", agentParams);
                Console.WriteLine(OutputFormatter.FormatAgent(agentData, jsonOutput));
            }
            else
            {
                var agentsData = client.Query("agents");
                var agentsList = ExtractListFromDictionary(agentsData, "agents");

                if (jsonOutput)
                {
                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(agentsList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    foreach (var agent in agentsList)
                    {
                        Console.WriteLine(OutputFormatter.FormatAgent(agent, jsonOutput));
                        Console.WriteLine("---");
                    }
                }
            }

            return 0;
        }

        private static int QueryCreatures(ITcpClient client, string? idArg, bool jsonOutput)
        {
            if (idArg != null)
            {
                var creatureParams = new Dictionary<string, object> { { "id", int.Parse(idArg) } };
                var creatureData = client.Query("creatures", creatureParams);
                Console.WriteLine(OutputFormatter.FormatCreature(creatureData, jsonOutput));
            }
            else
            {
                var creaturesData = client.Query("creatures");
                var creaturesList = ExtractListFromDictionary(creaturesData, "creatures");

                if (jsonOutput)
                {
                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(creaturesList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                }
                else
                {
                    foreach (var creature in creaturesList)
                    {
                        Console.WriteLine(OutputFormatter.FormatCreature(creature, jsonOutput));
                        Console.WriteLine("---");
                    }
                }
            }

            return 0;
        }

        private static int QueryGame(ITcpClient client, bool jsonOutput)
        {
            var gameData = client.Query("game");
            Console.WriteLine(OutputFormatter.FormatGameState(gameData, jsonOutput));
            return 0;
        }

        private static int QueryDepartments(ITcpClient client, bool jsonOutput)
        {
            var sefiraData = client.Query("sefira");
            var sefiraList = ExtractListFromDictionary(sefiraData, "sefiras");

            if (jsonOutput)
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(sefiraList, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                foreach (var sefira in sefiraList)
                {
                    Console.WriteLine(OutputFormatter.FormatDepartment(sefira, jsonOutput));
                    Console.WriteLine("---");
                }
            }

            return 0;
        }

        private static int QueryUi(ITcpClient client, bool jsonOutput, string[] args)
        {
            var depth = GetArgValue(args, "--depth") ?? "full";
            var windowName = GetArgValue(args, "--name");
            var uiParams = new Dictionary<string, object>();

            if (depth != "full")
            {
                uiParams["depth"] = depth;
            }

            if (windowName != null)
            {
                uiParams["name"] = windowName;
            }

            var uiData = client.Query("ui", uiParams);

            if (jsonOutput)
            {
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(uiData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine(OutputFormatter.FormatUiState(uiData));
            }

            return 0;
        }

        private static int PrintUnknownTargetError(string target)
        {
            Console.Error.WriteLine($"Unknown target: {target}");
            Console.Error.WriteLine("Valid targets: agents, creatures, game, departments, ui");
            return 1;
        }

        private static List<Dictionary<string, object>> ExtractListFromDictionary(Dictionary<string, object> data, string key)
        {
            return data.TryGetValue(key, out var value) && value is List<Dictionary<string, object>> list
                ? list
                : [];
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet playwright query <target> [id] [options]");
            Console.WriteLine();
            Console.WriteLine("Targets:");
            Console.WriteLine("  agents [id]        Query all agents or a specific agent by ID");
            Console.WriteLine("  creatures [id]      Query all abnormalities or a specific one by ID");
            Console.WriteLine("  game                Query game state overview");
            Console.WriteLine("  departments         Query department status");
            Console.WriteLine("  ui                  Query UI accessibility tree (how the agent \"sees\" the game)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --host HOST         Connect to specific host (default: localhost)");
            Console.WriteLine("  --port PORT         Connect to specific port (default: from config)");
            Console.WriteLine("  --json              Output raw JSON instead of formatted text");
            Console.WriteLine();
            Console.WriteLine("UI query options:");
            Console.WriteLine("  --depth summary     Show window states only (Tier 1)");
            Console.WriteLine("  --depth full        Show windows + child elements (default, Tier 1 + Tier 2)");
            Console.WriteLine("  --name <window>     Query only the specified window (e.g., AgentInfoWindow)");
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
