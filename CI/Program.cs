// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CI
{
    public sealed class ProcessResult
    {
        public int ExitCode { get; init; }
        public IList<string> ErrorLines { get; init; } = [];
    }

    public interface IProcessRunner
    {
        ProcessResult Run(string fileName, string arguments, string? workingDirectory = null, Func<string?, bool>? outputFilter = null);
    }

    public class ProcessRunner : IProcessRunner
    {
        public ProcessResult Run(string fileName, string arguments, string? workingDirectory = null, Func<string?, bool>? outputFilter = null)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (workingDirectory != null)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            var errorLines = new List<string>();

            try
            {
                using var process = new Process { StartInfo = processStartInfo };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null && (outputFilter == null || outputFilter(e.Data)))
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null && (outputFilter == null || outputFilter(e.Data)))
                    {
                        Console.Error.WriteLine(e.Data);
                        errorLines.Add(e.Data);
                    }
                };

                _ = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return new ProcessResult { ExitCode = process.ExitCode, ErrorLines = errorLines };
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // If the process fails to start (e.g., command not found), return a non-zero exit code
                return new ProcessResult { ExitCode = 1, ErrorLines = errorLines };
            }
            catch (FileNotFoundException)
            {
                // If the process file is not found, return a non-zero exit code
                return new ProcessResult { ExitCode = 1, ErrorLines = errorLines };
            }
            catch (InvalidOperationException)
            {
                // If there's an invalid operation starting the process, return a non-zero exit code
                return new ProcessResult { ExitCode = 1, ErrorLines = errorLines };
            }
        }
    }

    public interface IGitHookSetup
    {
        void SetupPreCommitHook(string repoRoot);
    }

    public class GitHookSetup(IFileSystem fileSystem) : IGitHookSetup
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public GitHookSetup()
            : this(new FileSystem())
        {
        }

        public void SetupPreCommitHook(string repoRoot)
        {
            var hookPath = Path.Combine(repoRoot, ".git", "hooks", "pre-commit");
            var hookContent = GetHookContent();

            _fileSystem.WriteAllText(hookPath, hookContent);

            // Set as executable on Unix systems
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                _fileSystem.SetFileExecutable(hookPath);
            }
        }

        private static string GetHookContent()
        {
            return @"#!/usr/bin/env bash
# Pre-commit hook that runs CI checks
set -euo pipefail

SCRIPT_DIR=""$(cd ""$(dirname ""$0"")"" && pwd)""
REPO_ROOT=""$(cd ""$SCRIPT_DIR/../.."" && pwd)""

echo ""Running pre-commit checks...""
cd ""$REPO_ROOT""
dotnet ci --check
";
        }
    }

    public interface IFileSystem
    {
        void WriteAllText(string path, string contents);
        void SetFileExecutable(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string? ReadAllText(string path);
    }

    public class FileSystem : IFileSystem
    {
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public string? ReadAllText(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public void SetFileExecutable(string path)
        {
            // Use chmod to make the file executable on Unix systems
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = "+x \"" + path + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            _ = process.Start();
            process.WaitForExit();
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }

    public class CiRunner(IProcessRunner processRunner, IGitHookSetup gitHookSetup, IFileSystem fileSystem, ICoverageThresholdChecker coverageThresholdChecker, string? baseDirectory)
    {
        private readonly IProcessRunner _processRunner = processRunner;
        private readonly IGitHookSetup _gitHookSetup = gitHookSetup;
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly ICoverageThresholdChecker _coverageThresholdChecker = coverageThresholdChecker;
        private readonly string? _baseDirectory = baseDirectory;

        public CiRunner()
            : this(new ProcessRunner(), new GitHookSetup(), new FileSystem(), new CoverageThresholdChecker())
        {
        }

        public CiRunner(IProcessRunner processRunner, IGitHookSetup gitHookSetup, IFileSystem fileSystem)
            : this(processRunner, gitHookSetup, fileSystem, new CoverageThresholdChecker())
        {
        }

        public CiRunner(IProcessRunner processRunner, IGitHookSetup gitHookSetup, IFileSystem fileSystem, ICoverageThresholdChecker coverageThresholdChecker)
            : this(processRunner, gitHookSetup, fileSystem, coverageThresholdChecker, null)
        {
        }

        public int Run(bool checkMode)
        {
            var repoRoot = FindRepositoryRoot();
            if (repoRoot == null)
            {
                Console.Error.WriteLine("Error: Could not find .git directory. Are you in a git repository?");
                return 1;
            }

            Console.WriteLine($"=== Running dotnet format ({(checkMode ? "verify" : "fix")}) ===");

            var formatArgs = checkMode
                ? "format --verify-no-changes LobotomyCorporationMods.sln"
                : "format LobotomyCorporationMods.sln";

            // Filter out the benign dotnet format workspace loading warning
            var formatResult = _processRunner.Run("dotnet", formatArgs, repoRoot, outputFilter: line =>
            {
                if (line != null && line.Contains("Warnings were encountered while loading the workspace", StringComparison.Ordinal))
                {
                    return false;
                }
                return true;
            });

            if (formatResult.ExitCode != 0)
            {
                ReplayErrors("FORMAT FAILED", formatResult.ErrorLines);
                return formatResult.ExitCode;
            }

            Console.WriteLine("=== Running dotnet test ===");
            var testArgs = "test /p:CollectCoverage=true /p:CoverletOutput=\"./coverage.opencover.xml\" /p:CoverletOutputFormat=opencover --verbosity normal --blame-hang-timeout 10s LobotomyCorporationMods.sln";
            var testResult = _processRunner.Run("dotnet", testArgs, repoRoot);

            if (testResult.ExitCode != 0)
            {
                ReplayErrors("TESTS FAILED", testResult.ErrorLines);
                return testResult.ExitCode;
            }

            // Check coverage thresholds
            Console.WriteLine("=== Checking coverage thresholds ===");
            if (_coverageThresholdChecker.CheckThresholds(repoRoot, out var failureMessage))
            {
                Console.WriteLine("=== Coverage thresholds met ===");
                return 0;
            }
            else
            {
                Console.Error.WriteLine("=== Coverage thresholds NOT met ===");
                Console.Error.WriteLine(failureMessage);
                return 1;
            }
        }

        public void SetupHooks()
        {
            var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("Error: Could not find .git directory. Are you in a git repository?");
            var hookPath = Path.Combine(repoRoot, ".git", "hooks", "pre-commit");
            Console.WriteLine($"Setting up pre-commit hook at {hookPath}...");

            _gitHookSetup.SetupPreCommitHook(repoRoot);

            Console.WriteLine("Pre-commit hook installed successfully!");
        }

        private static void ReplayErrors(string banner, IList<string> errorLines)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"=== {banner} ===");

            if (errorLines.Count > 0)
            {
                foreach (var line in errorLines)
                {
                    Console.Error.WriteLine(line);
                }
            }
        }

        private string? FindRepositoryRoot()
        {
            var currentDir = _baseDirectory ?? Directory.GetCurrentDirectory();
            var dir = new DirectoryInfo(currentDir);

            while (dir != null)
            {
                if (_fileSystem.DirectoryExists(Path.Combine(dir.FullName, ".git")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }
    }

    public sealed class Program
    {
        public static int Main(string[] args)
        {
            args ??= [];

            var checkMode = false;
            var setupHooks = false;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--check":
                        checkMode = true;
                        break;
                    case "--setup-hooks":
                        setupHooks = true;
                        break;
                    default:
                        Console.Error.WriteLine($"Error: Unknown argument '{arg}'");
                        Console.Error.WriteLine("Usage: CI [--check] [--setup-hooks]");
                        Console.Error.WriteLine("  --check         Run in verification mode (no auto-fix)");
                        Console.Error.WriteLine("  --setup-hooks   Install pre-commit git hook");
                        return 1;
                }
            }

            var runner = new CiRunner();

            if (setupHooks)
            {
                try
                {
                    runner.SetupHooks();
                    return 0;
                }
                catch (InvalidOperationException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }
            }

            return runner.Run(checkMode);
        }
    }
}
