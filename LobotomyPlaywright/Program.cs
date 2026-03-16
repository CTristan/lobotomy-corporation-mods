// SPDX-License-Identifier: MIT

using System;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Infrastructure;

// Only exclude specific classes from coverage, not the entire assembly
// [assembly: ExcludeFromCodeCoverage]

namespace LobotomyPlaywright
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            args ??= [];

            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            var command = args[0].ToLower(System.Globalization.CultureInfo.InvariantCulture);
            var commandArgs = args[1..];

            try
            {
                return command switch
                {
                    "find-game" => new FindGameCommand().Run(commandArgs),
                    "deploy" => new DeployCommand().Run(commandArgs),
                    "launch" => new LaunchCommand().Run(commandArgs),
                    "status" => new StatusCommand().Run(commandArgs),
                    "stop" => new StopCommand().Run(commandArgs),
                    "query" => new QueryCommand().Run(commandArgs),
                    "read-log" => new ReadLogCommand().Run(commandArgs),
                    "wait" => new WaitCommand().Run(commandArgs),
                    "screenshot" => new ScreenshotCommand().Run(commandArgs),
                    "command" => new CommandCommand().Run(commandArgs),
                    "switch-environment" => new SwitchEnvironmentCommand().Run(commandArgs),
                    _ => HandleUnknownCommand(command),
                };
            }
            catch (Exception ex)
            {
                ExceptionHelper.LogAndAttemptShutdown(ex, "Main");
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
            Console.WriteLine("    --profile <name>      Use a deployment profile (restores to vanilla first)");
            Console.WriteLine("    --full                Full game restore instead of targeted (use with --profile)");
            Console.WriteLine();
            Console.WriteLine("  Profiles:");
            Console.WriteLine("    vanilla               Clean game, no mods");
            Console.WriteLine("    lmm                   Game + LMM (Lobotomy Mod Manager)");
            Console.WriteLine("    bepinex               Game + BepInEx");
            Console.WriteLine("    mods                  LMM + all gameplay mods");
            Console.WriteLine("    mods-playwright       LMM + all gameplay mods + Playwright plugin");
            Console.WriteLine("    playwright            LMM + BepInEx + Playwright + RetargetHarmony");
            Console.WriteLine("    all                   Everything");
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
}
