// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Network;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Command to send commands to the game.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of CommandCommand class.
    /// </remarks>
    /// <param name="configManager">The config manager.</param>
    /// <param name="tcpClientFactory">Factory for creating TCP clients.</param>
    public class CommandCommand(IConfigManager configManager, Func<ITcpClient> tcpClientFactory)
    {
        private readonly IConfigManager _configManager = configManager;
        private readonly Func<ITcpClient> _tcpClientFactory = tcpClientFactory;

        /// <summary>
        /// Initializes a new instance of CommandCommand class with default implementations.
        /// </summary>
        public CommandCommand()
            : this(new ConfigManager(new FileSystem()), () => new PlaywrightTcpClient())
        {
        }

        /// <summary>
        /// Runs the command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var command = args[0].ToLowerInvariant();
            var commandArgs = args[1..];

            // Map aliases
            command = command switch
            {
                "assign-work" => "assign-work",
                "set-agent-stats" => "set-agent-stats",
                "add-gift" => "add-gift",
                "remove-gift" => "remove-gift",
                "set-qliphoth" => "set-qliphoth",
                "fill-energy" => "fill-energy",
                "set-game-speed" => "set-game-speed",
                "set-agent-invincible" => "set-agent-invincible",
                "pause" => "pause",
                "unpause" => "unpause",
                "deploy-agent" => "deploy-agent",
                "recall-agent" => "recall-agent",
                "suppress" => "suppress",
                "help" or "--help" or "-h" => "help",
                _ => command
            };

            if (command == "help")
            {
                PrintUsage();
                return 0;
            }

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

            var host = GetArgValue(commandArgs, "--host") ?? "localhost";
            var portArg = GetArgValue(commandArgs, "--port");
            int port;
            try
            {
                port = portArg != null ? int.Parse(portArg) : config.TcpPort;
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --port must be a valid number");
                return 1;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --port value is too large");
                return 1;
            }

            // Build parameters based on command
            var parameters = BuildParameters(command, commandArgs);
            if (parameters == null)
            {
                return 1; // Error already printed
            }

            try
            {
                using var client = _tcpClientFactory();
                client.Connect(host, port);

                var responseData = client.SendCommandWithData(command, parameters);

                if (responseData == null)
                {
                    Console.Error.WriteLine("ERROR: No response from server");
                    return 1;
                }

                // Display response
                DisplayResponse(responseData);

                return 0;
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Net.Sockets.SocketException)
            {
                Console.Error.WriteLine($"Connection error: {ex.Message}");
                Console.Error.WriteLine("Ensure Lobotomy Corporation is running with LobotomyPlaywright plugin.");
                return 1;
            }
        }

        private Dictionary<string, object>? BuildParameters(string command, string[] args)
        {
            _ = new Dictionary<string, object>();

            switch (command)
            {
                case "set-agent-stats":
                    return BuildSetAgentStatsParams(args);

                case "add-gift":
                case "remove-gift":
                    return BuildGiftParams(args);

                case "set-qliphoth":
                    return BuildSetQliphothParams(args);

                case "set-game-speed":
                    return BuildSetGameSpeedParams(args);

                case "set-agent-invincible":
                    return BuildSetAgentInvincibleParams(args);

                case "assign-work":
                    return BuildAssignWorkParams(args);

                case "deploy-agent":
                    return BuildDeployAgentParams(args);

                case "recall-agent":
                    return BuildRecallAgentParams(args);

                case "suppress":
                    return BuildSuppressParams(args);

                case "pause":
                case "unpause":
                    return [];

                case "fill-energy":
                    return [];

                default:
                    Console.Error.WriteLine($"ERROR: Unknown command: {command}");
                    return null;
            }
        }

        private Dictionary<string, object>? BuildSetAgentStatsParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");
            var hp = GetArgValue(args, "--hp");
            var mental = GetArgValue(args, "--mental");
            var fortitude = GetArgValue(args, "--fortitude");
            var prudence = GetArgValue(args, "--prudence");
            var temperance = GetArgValue(args, "--temperance");
            var justice = GetArgValue(args, "--justice");

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required for set-agent-stats");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
                if (!string.IsNullOrEmpty(hp))
                {
                    parameters["hp"] = float.Parse(hp);
                }

                if (!string.IsNullOrEmpty(mental))
                {
                    parameters["mental"] = float.Parse(mental);
                }

                if (!string.IsNullOrEmpty(fortitude))
                {
                    parameters["fortitude"] = int.Parse(fortitude);
                }

                if (!string.IsNullOrEmpty(prudence))
                {
                    parameters["prudence"] = int.Parse(prudence);
                }

                if (!string.IsNullOrEmpty(temperance))
                {
                    parameters["temperance"] = int.Parse(temperance);
                }

                if (!string.IsNullOrEmpty(justice))
                {
                    parameters["justice"] = int.Parse(justice);
                }
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: Parameter values must be valid numbers");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: Parameter values are too large");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildGiftParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");
            var giftId = GetArgValue(args, "--gift");

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required");
                return null;
            }

            if (string.IsNullOrEmpty(giftId))
            {
                Console.Error.WriteLine("ERROR: --gift is required");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
                parameters["giftId"] = int.Parse(giftId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --agent and --gift must be valid numbers");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: Parameter values are too large");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildSetQliphothParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var creatureId = GetArgValue(args, "--creature");
            var counter = GetArgValue(args, "--counter");

            if (string.IsNullOrEmpty(creatureId))
            {
                Console.Error.WriteLine("ERROR: --creature is required for set-qliphoth");
                return null;
            }

            if (string.IsNullOrEmpty(counter))
            {
                Console.Error.WriteLine("ERROR: --counter is required for set-qliphoth");
                return null;
            }

            try
            {
                parameters["creatureId"] = long.Parse(creatureId);
                parameters["counter"] = int.Parse(counter);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --creature and --counter must be valid numbers");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: Parameter values are too large");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildSetGameSpeedParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var speed = GetArgValue(args, "--speed");

            if (string.IsNullOrEmpty(speed))
            {
                Console.Error.WriteLine("ERROR: --speed is required for set-game-speed");
                return null;
            }

            try
            {
                parameters["speed"] = int.Parse(speed);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --speed must be a valid number");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --speed value is too large");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildSetAgentInvincibleParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");
            var invincible = GetArgValue(args, "--invincible") ?? "true";

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required for set-agent-invincible");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --agent must be a valid number");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --agent value is too large");
                return null;
            }

            try
            {
                parameters["invincible"] = bool.Parse(invincible);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --invincible must be 'true' or 'false'");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildAssignWorkParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");
            var creatureId = GetArgValue(args, "--creature");
            var workType = GetArgValue(args, "--work") ?? GetArgValue(args, "--work-type");

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required for assign-work");
                return null;
            }

            if (string.IsNullOrEmpty(creatureId))
            {
                Console.Error.WriteLine("ERROR: --creature is required for assign-work");
                return null;
            }

            if (string.IsNullOrEmpty(workType))
            {
                Console.Error.WriteLine("ERROR: --work (or --work-type) is required for assign-work");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
                parameters["creatureId"] = long.Parse(creatureId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --agent and --creature must be valid numbers");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: Parameter values are too large");
                return null;
            }

            parameters["workType"] = workType;

            return parameters;
        }

        private Dictionary<string, object>? BuildDeployAgentParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");
            var sefira = GetArgValue(args, "--sefira") ?? GetArgValue(args, "--department");

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required for deploy-agent");
                return null;
            }

            if (string.IsNullOrEmpty(sefira))
            {
                Console.Error.WriteLine("ERROR: --sefira (or --department) is required for deploy-agent");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --agent must be a valid number");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --agent value is too large");
                return null;
            }

            parameters["sefira"] = sefira.ToUpperInvariant();

            return parameters;
        }

        private Dictionary<string, object>? BuildRecallAgentParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var agentId = GetArgValue(args, "--agent");

            if (string.IsNullOrEmpty(agentId))
            {
                Console.Error.WriteLine("ERROR: --agent is required for recall-agent");
                return null;
            }

            try
            {
                parameters["agentId"] = long.Parse(agentId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --agent must be a valid number");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --agent value is too large");
                return null;
            }

            return parameters;
        }

        private Dictionary<string, object>? BuildSuppressParams(string[] args)
        {
            var parameters = new Dictionary<string, object>();
            var creatureId = GetArgValue(args, "--creature");

            if (string.IsNullOrEmpty(creatureId))
            {
                Console.Error.WriteLine("ERROR: --creature is required for suppress");
                return null;
            }

            try
            {
                parameters["creatureId"] = long.Parse(creatureId);
            }
            catch (FormatException)
            {
                Console.Error.WriteLine("ERROR: --creature must be a valid number");
                return null;
            }
            catch (OverflowException)
            {
                Console.Error.WriteLine("ERROR: --creature value is too large");
                return null;
            }

            return parameters;
        }

        private void DisplayResponse(Dictionary<string, object> responseData)
        {
            var result = GetStringValue(responseData.GetValueOrDefault("result"));
            var error = GetStringValue(responseData.GetValueOrDefault("error"));

            if (!string.IsNullOrEmpty(error))
            {
                Console.Error.WriteLine($"ERROR: {error}");
                return;
            }

            if (!string.IsNullOrEmpty(result))
            {
                Console.WriteLine($"Command succeeded: {result}");
            }

            // Print other fields
            foreach (var kvp in responseData)
            {
                if (kvp.Key is not "result" and not "error")
                {
                    Console.WriteLine($"  {kvp.Key}: {GetStringValue(kvp.Value) ?? "null"}");
                }
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

        private static string? GetStringValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToString();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet playwright command <action> [options]");
            Console.WriteLine();
            Console.WriteLine("Debug Commands (state manipulation):");
            Console.WriteLine("  set-agent-stats --agent <id> [--hp <value>] [--mental <value>]");
            Console.WriteLine("                    [--fortitude <value>] [--prudence <value>]");
            Console.WriteLine("                    [--temperance <value>] [--justice <value>]");
            Console.WriteLine("  add-gift --agent <id> --gift <gift-id>");
            Console.WriteLine("  remove-gift --agent <id> --gift <gift-id>");
            Console.WriteLine("  set-qliphoth --creature <id> --counter <value>");
            Console.WriteLine("  fill-energy");
            Console.WriteLine("  set-game-speed --speed <1-5>");
            Console.WriteLine("  set-agent-invincible --agent <id> [--invincible true|false]");
            Console.WriteLine();
            Console.WriteLine("Player Action Simulation:");
            Console.WriteLine("  pause");
            Console.WriteLine("  unpause");
            Console.WriteLine("  assign-work --agent <id> --creature <id> --work <work-type>");
            Console.WriteLine("  deploy-agent --agent <id> --sefira <department>");
            Console.WriteLine("  recall-agent --agent <id>");
            Console.WriteLine("  suppress --creature <id>");
            Console.WriteLine();
            Console.WriteLine("Common Options:");
            Console.WriteLine("  --host <host>     TCP host (default: localhost)");
            Console.WriteLine("  --port <port>    TCP port (default: from config)");
            Console.WriteLine();
            Console.WriteLine("Work types: instinct, insight, attachment, repression");
            Console.WriteLine("Departments: BINAH, CHESED, GEBURAH, TIPHERETH, NETZACH, YESOD, MALKUTH");
        }
    }
}
