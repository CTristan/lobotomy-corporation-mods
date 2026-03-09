// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyPlaywright.Commands;

// Only exclude specific classes from coverage, not the entire assembly
// [assembly: ExcludeFromCodeCoverage]

namespace LobotomyPlaywright;

#pragma warning disable RCS1102 // Make class static
#pragma warning disable CA1515 // Types need to be public for testability
#pragma warning disable CA1311 // Specify a culture or use an invariant version

public sealed class Program
{
    public static int Main(string[] args)
    {
        args ??= Array.Empty<string>();

        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].ToLower(System.Globalization.CultureInfo.InvariantCulture);
        var commandArgs = args[1..];

        try
        {
            switch (command)
            {
                case "find-game":
                    return new FindGameCommand().Run(commandArgs);
                case "deploy":
                    return new DeployCommand().Run(commandArgs);
                case "launch":
                    return new LaunchCommand().Run(commandArgs);
                case "status":
                    return new StatusCommand().Run(commandArgs);
                case "stop":
                    return new StopCommand().Run(commandArgs);
                case "query":
                    return new QueryCommand().Run(commandArgs);
                case "read-log":
                    return new ReadLogCommand().Run(commandArgs);
                case "wait":
                    return new WaitCommand().Run(commandArgs);
                case "screenshot":
                    return new ScreenshotCommand().Run(commandArgs);
                case "command":
                    return new CommandCommand().Run(commandArgs);
                case "switch-environment":
                    return new SwitchEnvironmentCommand().Run(commandArgs);
                default:
                    return HandleUnknownCommand(command);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine();
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("LobotomyPlaywright CLI - Game automation and state querying");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet playwright <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  find-game               Auto-detect and configure game installation path");
        Console.WriteLine("  deploy                  Build and deploy plugin DLLs to game");
        Console.WriteLine("  launch                  Launch game and wait for TCP readiness");
        Console.WriteLine("  status                  Check game status");
        Console.WriteLine("  stop                    Stop the game");
        Console.WriteLine("  switch-environment      Switch between debug/release game environments");
        Console.WriteLine("  query <target>          Query game state");
        Console.WriteLine("  read-log                Read BepInEx log files");
        Console.WriteLine("  wait event <names>      Wait for specific game events");
        Console.WriteLine("  screenshot              Capture a screenshot of the current game state");
        Console.WriteLine("  command <action>        Send commands to the game");
        Console.WriteLine();
        Console.WriteLine("Use 'dotnet playwright <command> --help' for command-specific help.");
    }
}
