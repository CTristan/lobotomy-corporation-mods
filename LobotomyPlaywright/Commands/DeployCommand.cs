// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to build and deploy the plugin DLLs to the game.
/// </summary>
internal class DeployCommand
{
    private const string PluginDllName = "LobotomyPlaywright.Plugin.dll";
    private const string RetargetHarmonyDllName = "RetargetHarmony.dll";
    private static readonly string[] HarmonyInteropDlls = { "0Harmony109.dll", "0Harmony12.dll" };

    private readonly IConfigManager _configManager;
    private readonly IFileSystem _fileSystem;
    private readonly IProcessRunner _processRunner;

    /// <summary>
    /// Initializes a new instance of DeployCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="processRunner">The process runner.</param>
    public DeployCommand(IConfigManager configManager, IFileSystem fileSystem, IProcessRunner processRunner)
    {
        _configManager = configManager;
        _fileSystem = fileSystem;
        _processRunner = processRunner;
    }

    /// <summary>
    /// Initializes a new instance of DeployCommand class with default implementations.
    /// </summary>
    public DeployCommand()
        : this(new ConfigManager(new FileSystem()), new FileSystem(), new ProcessRunner())
    {
    }

    /// <summary>
    /// Runs the deploy command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        var configuration = GetArgValue(args, "--configuration") ?? "Release";
        var skipBuild = HasArg(args, "--skip-build");
        var dryRun = HasArg(args, "--dry-run");

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

        var gamePath = config.GamePath;

        if (!_fileSystem.DirectoryExists(gamePath))
        {
            Console.Error.WriteLine($"ERROR: Game path does not exist: {gamePath}");
            Console.Error.WriteLine("The volume may not be mounted. Run 'dotnet playwright find-game' to reconfigure.");
            return 1;
        }

        // Repository root
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("Could not find repository root");

        var pluginProject = Path.Combine(repoRoot, "LobotomyPlaywright.Plugin", "LobotomyPlaywright.Plugin.csproj");
        var retharmonyProject = Path.Combine(repoRoot, "RetargetHarmony", "RetargetHarmony.csproj");

        if (!_fileSystem.FileExists(pluginProject))
        {
            Console.Error.WriteLine($"ERROR: Plugin project not found: {pluginProject}");
            return 1;
        }

        if (!_fileSystem.FileExists(retharmonyProject))
        {
            Console.Error.WriteLine($"ERROR: RetargetHarmony project not found: {retharmonyProject}");
            return 1;
        }

        // Build projects
        string pluginDllPath;
        string retharmonyDllPath;

        if (!skipBuild)
        {
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Building Projects");
            Console.WriteLine("".PadRight(60, '='));

            try
            {
                pluginDllPath = BuildProject(pluginProject, configuration);
                retharmonyDllPath = BuildProject(retharmonyProject, configuration);
            }
            catch (Exception ex) when (ex is BuildFailedException || ex is FileNotFoundException)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"ERROR: Build failed: {ex.Message}");
                return 1;
            }
        }
        else
        {
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Skipping Build (using existing DLLs)");
            Console.WriteLine("".PadRight(60, '='));

            pluginDllPath = Path.Combine(
                Path.GetDirectoryName(pluginProject) ?? string.Empty,
                "bin",
                configuration,
                "net35",
                PluginDllName
            );

            retharmonyDllPath = Path.Combine(
                Path.GetDirectoryName(retharmonyProject) ?? string.Empty,
                "bin",
                configuration,
                "net35",
                RetargetHarmonyDllName
            );

            if (!_fileSystem.FileExists(pluginDllPath))
            {
                Console.Error.WriteLine($"ERROR: Plugin DLL not found: {pluginDllPath}");
                Console.Error.WriteLine("Run without --skip-build to build the project first.");
                return 1;
            }

            if (!_fileSystem.FileExists(retharmonyDllPath))
            {
                Console.Error.WriteLine($"ERROR: RetargetHarmony DLL not found: {retharmonyDllPath}");
                Console.Error.WriteLine("Run without --skip-build to build the project first.");
                return 1;
            }

            Console.WriteLine($"Using existing plugin DLL: {pluginDllPath}");
            Console.WriteLine($"Using existing RetargetHarmony DLL: {retharmonyDllPath}");
        }

        // Deploy DLLs
        Console.WriteLine();
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine("Deploying DLLs");
        Console.WriteLine("".PadRight(60, '='));

        if (dryRun)
        {
            Console.WriteLine();
            Console.WriteLine($"Would deploy {PluginDllName} to:");
            Console.WriteLine($"  {Path.Combine(gamePath, "BepInEx", "plugins", PluginDllName)}");
            Console.WriteLine();
            Console.WriteLine($"Would deploy {RetargetHarmonyDllName} to:");
            Console.WriteLine($"  {Path.Combine(gamePath, "BepInEx", "patchers", RetargetHarmonyDllName)}");
            foreach (var dllName in HarmonyInteropDlls)
            {
                Console.WriteLine();
                Console.WriteLine($"Would deploy {dllName} to:");
                Console.WriteLine($"  {Path.Combine(gamePath, "BepInEx", "core", dllName)}");
            }

            Console.WriteLine();
            Console.WriteLine("Dry run complete. Remove --dry-run to actually deploy.");
            return 0;
        }

        try
        {
            var deployPluginPath = DeployDll(pluginDllPath, gamePath, "plugins");
            var deployRetharmonyPath = DeployDll(retharmonyDllPath, gamePath, "patchers");

            Console.WriteLine();
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Deployment Summary");
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine($"Plugin: {deployPluginPath}");
            Console.WriteLine($"Size: {new FileInfo(deployPluginPath).Length:N0} bytes");
            Console.WriteLine($"RetargetHarmony: {deployRetharmonyPath}");
            Console.WriteLine($"Size: {new FileInfo(deployRetharmonyPath).Length:N0} bytes");

            foreach (var dllName in HarmonyInteropDlls)
            {
                DeployInteropDll(repoRoot, dllName, gamePath);
            }

            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine();
            Console.WriteLine("Deployment successful!");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  dotnet playwright launch   # Launch the game");
            Console.WriteLine("  dotnet playwright status   # Check game status");

            return 0;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine($"ERROR: Deployment failed: {ex.Message}");
            return 1;
        }
    }

    private string BuildProject(string projectPath, string configuration)
    {
        var projectName = Path.GetFileNameWithoutExtension(projectPath);
        Console.WriteLine();
        Console.WriteLine($"Building {projectName}...");

        var result = _processRunner.Run(
            "dotnet",
            $"build \"{projectPath}\" --configuration {configuration}",
            Path.GetDirectoryName(projectPath),
            null
        );

        if (result != 0)
        {
            throw new BuildFailedException($"Build failed for {projectName}");
        }

        // Find the output DLL
        var projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        var dllPath = Path.Combine(projectDir, "bin", configuration, "net35", projectName, $"{projectName}.dll");

        if (!_fileSystem.FileExists(dllPath))
        {
            // Try alternate naming
            dllPath = Path.Combine(projectDir, "bin", configuration, "net35", $"{projectName}.dll");
        }

        if (!_fileSystem.FileExists(dllPath))
        {
            throw new FileNotFoundException($"Built DLL not found: {dllPath}");
        }

        Console.WriteLine($"Built: {dllPath}");
        Console.WriteLine($"Size: {new FileInfo(dllPath).Length:N0} bytes");

        return dllPath;
    }

    private string DeployDll(string sourceDll, string gamePath, string destSubdir)
    {
        var bepinexPath = Path.Combine(gamePath, "BepInEx");
        var destDir = Path.Combine(bepinexPath, destSubdir);
        var destDll = Path.Combine(destDir, Path.GetFileName(sourceDll) ?? string.Empty);

        Console.WriteLine();
        Console.WriteLine($"Deploying {Path.GetFileName(sourceDll)} to {destDir}...");

        // Create destination directory if it doesn't exist
        if (!_fileSystem.DirectoryExists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // Copy the DLL
        File.Copy(sourceDll, destDll, true);

        // Verify deployment
        if (!_fileSystem.FileExists(destDll))
        {
            throw new IOException($"Failed to copy {Path.GetFileName(sourceDll)} to {destDll}");
        }

        var fileInfo = new FileInfo(destDll);
        if (fileInfo.Length == 0)
        {
            throw new IOException($"Deployed DLL is empty: {destDll}");
        }

        Console.WriteLine($"Deployed: {destDll}");
        Console.WriteLine($"Size: {fileInfo.Length:N0} bytes");

        return destDll;
    }

    private void DeployInteropDll(string repoRoot, string dllName, string gamePath)
    {
        var sourceDll = Path.Combine(repoRoot, "RetargetHarmony", "lib", dllName);

        if (!_fileSystem.FileExists(sourceDll))
        {
            throw new FileNotFoundException(
                $"Harmony interop DLL not found: {sourceDll}\n" +
                $"Expected vendored DLLs from BepInEx/HarmonyInteropDlls in RetargetHarmony/lib/");
        }

        var destPath = DeployDll(sourceDll, gamePath, "core");
        Console.WriteLine($"Interop: {destPath}");
        Console.WriteLine($"Size: {new FileInfo(destPath).Length:N0} bytes");
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (_fileSystemStatic.DirectoryExists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            if (_fileSystemStatic.FileExists(Path.Combine(dir.FullName, "LobotomyCorporationMods.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static readonly FileSystem _fileSystemStatic = new();

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

    private class BuildFailedException : Exception
    {
        public BuildFailedException(string message) : base(message)
        {
        }

        public BuildFailedException()
        {
        }

        public BuildFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
