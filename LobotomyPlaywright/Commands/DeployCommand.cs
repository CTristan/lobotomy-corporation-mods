// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Commands
{
    /// <summary>
    /// Command to build and deploy the plugin DLLs to the game.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of DeployCommand class.
    /// </remarks>
    /// <param name="configManager">The config manager.</param>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="processRunner">The process runner.</param>
    /// <param name="gameRestorer">The game restorer.</param>
    /// <param name="lmmInstaller">The LMM installer.</param>
    /// <param name="bepInExInstaller">The BepInEx installer.</param>
    /// <param name="profileLoader">The profile loader.</param>
    public class DeployCommand(IConfigManager configManager, IFileSystem fileSystem, IProcessRunner processRunner, IGameRestorer gameRestorer, ILmmInstaller lmmInstaller, IBepInExInstaller bepInExInstaller, IProfileLoader profileLoader)
    {
        private static readonly string[] s_harmonyInteropDlls = { "0Harmony109.dll", "0Harmony12.dll", "12Harmony.dll" };
        private static readonly string[] s_modContentDirs = { "Info", "Assets", "Localize", "Data" };
        private const string ThirdPartyModsRelativePath = "external/thirdparty-mods";

        private static readonly DeploymentTarget[] s_deploymentTargets =
        [
            new("LobotomyCorporationMods.Playwright", "Hemocode.Playwright", "BaseMods/Hemocode.Playwright", true),
            new("RetargetHarmony", "RetargetHarmony", "patchers/RetargetHarmony", false),
            new("LobotomyCorporationMods.BadLuckProtectionForGifts", "Hemocode.BadLuckProtectionForGifts", "BaseMods/Hemocode.BadLuckProtectionForGifts", true),
            new("LobotomyCorporationMods.BugFixes", "Hemocode.BugFixes", "BaseMods/Hemocode.BugFixes", true),
            new("LobotomyCorporationMods.DebugPanel", "Hemocode.DebugPanel", "BaseMods/Hemocode.DebugPanel", true),
            new("LobotomyCorporationMods.FreeCustomization", "Hemocode.FreeCustomization", "BaseMods/Hemocode.FreeCustomization", true),
            new("LobotomyCorporationMods.GiftAlertIcon", "Hemocode.GiftAlertIcon", "BaseMods/Hemocode.GiftAlertIcon", true),
            new("LobotomyCorporationMods.NotifyWhenAgentReceivesGift", "Hemocode.NotifyWhenAgentReceivesGift", "BaseMods/Hemocode.NotifyWhenAgentReceivesGift", true),
            new("LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking", "Hemocode.WarnWhenAgentWillDieFromWorking", "BaseMods/Hemocode.WarnWhenAgentWillDieFromWorking", true),
            new("DemoMod.Plugin", "DemoMod.Plugin", "BaseMods/DemoMod.Plugin", true, "Harmony2ForLmm/DemoMod/DemoMod.Plugin/DemoMod.Plugin.csproj"),
            new("DemoMod.Patcher", "DemoMod.Patcher", "patchers/DemoMod.Patcher", false, "Harmony2ForLmm/DemoMod/DemoMod.Patcher/DemoMod.Patcher.csproj"),
        ];

        private readonly IConfigManager _configManager = configManager;
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly IProcessRunner _processRunner = processRunner;
        private readonly IGameRestorer _gameRestorer = gameRestorer;
        private readonly ILmmInstaller _lmmInstaller = lmmInstaller;
        private readonly IBepInExInstaller _bepInExInstaller = bepInExInstaller;
        private readonly IProfileLoader _profileLoader = profileLoader;

        /// <summary>
        /// Initializes a new instance of DeployCommand class with default implementations.
        /// </summary>
        public DeployCommand()
            : this(new ConfigManager(new FileSystem()), new FileSystem(), new ProcessRunner(), new GameRestorer(new FileSystem()), new LmmInstaller(new FileSystem()), new BepInExInstaller(new FileSystem()), new ProfileLoader(new FileSystem(), Path.GetFullPath("profiles.json")))
        {
        }

        /// <summary>
        /// Runs the deploy command.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public int Run(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            var configuration = GetArgValue(args, "--configuration") ?? "Release";
            var skipBuild = HasArg(args, "--skip-build");
            var dryRun = HasArg(args, "--dry-run");
            var profileName = GetArgValue(args, "--profile");
            var fullRestore = HasArg(args, "--full");

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

            if (!_fileSystem.DirectoryExists(gamePath))
            {
                Console.Error.WriteLine($"ERROR: Game path does not exist: {gamePath}");
                Console.Error.WriteLine("The volume may not be mounted. Run 'dotnet playwright find-game' to reconfigure.");
                return 1;
            }

            // Repository root
            var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("Could not find repository root");

            // Resolve profile if specified
            DeploymentProfile? profile = null;
            var targets = s_deploymentTargets;

            if (profileName is not null)
            {
                Dictionary<string, DeploymentProfile> profiles;
                try
                {
                    profiles = _profileLoader.Load();
                }
                catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException)
                {
                    Console.Error.WriteLine($"ERROR: {ex.Message}");
                    return 1;
                }

                if (!profiles.TryGetValue(profileName, out profile))
                {
                    Console.Error.WriteLine($"ERROR: Unknown profile '{profileName}'");
                    if (profiles.Count > 0)
                    {
                        Console.Error.WriteLine($"Available profiles: {string.Join(", ", profiles.Keys)}");
                    }

                    return 1;
                }

                // Restore game to vanilla state and install layers
                var snapshotPath = Path.Combine(repoRoot, "external", "snapshots");
                var lmmSourcePath = Path.Combine(snapshotPath, "LobotomyModManager");
                var vanillaPath = Path.Combine(snapshotPath, "LobotomyCorp_vanilla");

                if (!_fileSystem.DirectoryExists(vanillaPath))
                {
                    var vanillaManagedPath = Path.Combine(vanillaPath, "LobotomyCorp_Data", "Managed");
                    _fileSystem.CreateDirectory(vanillaManagedPath);

                    Console.Error.WriteLine();
                    Console.Error.WriteLine("".PadRight(60, '='));
                    Console.Error.WriteLine("Vanilla game snapshot required");
                    Console.Error.WriteLine("".PadRight(60, '='));
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Profile-based deployment needs a vanilla copy of the game's DLLs.");
                    Console.Error.WriteLine("The following directory has been created for you:");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"  {vanillaPath}");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Copy the vanilla game's LobotomyCorp_Data/Managed/ folder into it.");
                    Console.Error.WriteLine($"Your game is installed at: {gamePath}");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("If your game is currently unmodified, you can run:");
                    Console.Error.WriteLine($"  cp -r \"{Path.Combine(gamePath, "LobotomyCorp_Data")}\" \"{vanillaPath}/\"");
                    Console.Error.WriteLine();

                    return 1;
                }

                if (profile.InstallLmm && !_fileSystem.DirectoryExists(lmmSourcePath))
                {
                    _fileSystem.CreateDirectory(lmmSourcePath);

                    Console.Error.WriteLine();
                    Console.Error.WriteLine("".PadRight(60, '='));
                    Console.Error.WriteLine("LobotomyModManager installation required");
                    Console.Error.WriteLine("".PadRight(60, '='));
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("This profile requires LMM (Lobotomy Mod Manager) to patch the game.");
                    Console.Error.WriteLine("The following directory has been created for you:");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"  {lmmSourcePath}");
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Extract the LobotomyModManager tool into this directory.");
                    Console.Error.WriteLine("After extracting, the directory should contain LobotomyModManager_Data/PatchFiles/.");
                    Console.Error.WriteLine();

                    return 1;
                }

                if (dryRun)
                {
                    Console.WriteLine();
                    Console.WriteLine("".PadRight(60, '='));
                    Console.WriteLine($"Profile: {profileName}");
                    Console.WriteLine("".PadRight(60, '='));
                    Console.WriteLine($"Would restore game to vanilla state ({(fullRestore ? "full" : "targeted")})");
                    if (profile.InstallLmm)
                    {
                        Console.WriteLine("Would install LMM");
                    }

                    if (profile.InstallModLoader)
                    {
                        Console.WriteLine("Would install BepInEx");
                    }
                }
                else
                {
                    try
                    {
                        if (fullRestore)
                        {
                            _gameRestorer.RestoreFull(gamePath, snapshotPath);
                        }
                        else
                        {
                            _gameRestorer.RestoreTargeted(gamePath, snapshotPath);
                        }

                        // Install layers
                        if (profile.InstallLmm)
                        {
                            _lmmInstaller.Install(gamePath, lmmSourcePath);
                        }

                        if (profile.InstallModLoader)
                        {
                            var bepInExSourcePath = Path.Combine(repoRoot, "Harmony2ForLmm", "Resources", "bepinex");
                            _bepInExInstaller.Install(gamePath, bepInExSourcePath);
                        }
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
                    {
                        Console.Error.WriteLine();
                        Console.Error.WriteLine($"ERROR: Profile setup failed: {ex.Message}");
                        return 1;
                    }
                }

                // Filter deployment targets to profile
                targets = s_deploymentTargets
                    .Where(t => profile.DeployTargets.Contains(t.ProjectName))
                    .ToArray();
            }

            // If profile has no deploy targets, skip build and deploy phases
            if (targets.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("".PadRight(60, '='));
                Console.WriteLine("No deployment targets for this profile.");
                Console.WriteLine("".PadRight(60, '='));
                Console.WriteLine();
                Console.WriteLine("Deployment successful!");
                return 0;
            }

            // Validate all project files exist
            var projectPaths = new Dictionary<string, string>();
            foreach (var target in targets)
            {
                var projectPath = target.ProjectPath != null
                    ? Path.Combine(repoRoot, target.ProjectPath)
                    : Path.Combine(repoRoot, target.ProjectName, $"{target.ProjectName}.csproj");
                if (!_fileSystem.FileExists(projectPath))
                {
                    Console.Error.WriteLine($"ERROR: Project not found: {projectPath}");
                    return 1;
                }

                projectPaths[target.ProjectName] = projectPath;
            }

            // Build or locate DLLs
            var dllPaths = new Dictionary<string, string>();

            if (!skipBuild)
            {
                Console.WriteLine("".PadRight(60, '='));
                Console.WriteLine("Building Projects");
                Console.WriteLine("".PadRight(60, '='));

                try
                {
                    foreach (var target in targets)
                    {
                        dllPaths[target.ProjectName] = BuildProject(projectPaths[target.ProjectName], configuration, target.AssemblyName);
                    }
                }
                catch (Exception ex) when (ex is BuildFailedException or FileNotFoundException)
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

                foreach (var target in targets)
                {
                    var projectDir = Path.GetDirectoryName(projectPaths[target.ProjectName]) ?? string.Empty;
                    var dllName = $"{target.AssemblyName}.dll";
                    var dllPath = FindExistingDll(projectDir, configuration, dllName);

                    if (dllPath is null)
                    {
                        Console.Error.WriteLine($"ERROR: {target.ProjectName} DLL not found in build output.");
                        Console.Error.WriteLine("Run without --skip-build to build the project first.");
                        return 1;
                    }

                    dllPaths[target.ProjectName] = dllPath;
                    Console.WriteLine($"Using existing {target.ProjectName} DLL: {dllPath}");
                }
            }

            // Deploy DLLs
            Console.WriteLine();
            Console.WriteLine("".PadRight(60, '='));
            Console.WriteLine("Deploying DLLs");
            Console.WriteLine("".PadRight(60, '='));

            var deployInteropDlls = profile?.InstallModLoader != false;

            // Discover third-party mods if profile enables it
            var thirdPartyMods = new List<(string ModName, string DllPath, string ModDir)>();
            if (profile?.IncludeThirdPartyMods == true)
            {
                thirdPartyMods = DiscoverThirdPartyMods(repoRoot);
            }

            if (dryRun)
            {
                foreach (var target in targets)
                {
                    var deploySubdir = ResolveDeploySubdir(target, profile);
                    var destDir = GetDeployDestDir(gamePath, deploySubdir);
                    var dllName = $"{target.AssemblyName}.dll";
                    Console.WriteLine();
                    Console.WriteLine($"Would deploy {dllName} to:");
                    Console.WriteLine($"  {Path.Combine(destDir, dllName)}");

                    if (target.IsMod)
                    {
                        Console.WriteLine($"  + Common DLL and content directories");
                    }
                }

                foreach (var (modName, dllPath, _) in thirdPartyMods)
                {
                    var destDir = GetDeployDestDir(gamePath, $"BaseMods/{modName}");
                    Console.WriteLine();
                    Console.WriteLine($"Would deploy third-party mod {Path.GetFileName(dllPath)} to:");
                    Console.WriteLine($"  {Path.Combine(destDir, Path.GetFileName(dllPath))}");
                    Console.WriteLine($"  + content directories (if present)");
                }

                if (deployInteropDlls)
                {
                    foreach (var dllName in s_harmonyInteropDlls)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Would deploy {dllName} to:");
                        Console.WriteLine($"  {Path.Combine(gamePath, "BepInEx", "core", dllName)}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Dry run complete. Remove --dry-run to actually deploy.");
                return 0;
            }

            try
            {
                var deployedPaths = new Dictionary<string, string>();
                foreach (var target in targets)
                {
                    var deploySubdir = ResolveDeploySubdir(target, profile);
                    deployedPaths[target.ProjectName] = DeployDll(dllPaths[target.ProjectName], gamePath, deploySubdir);

                    if (target.IsMod)
                    {
                        DeployModContent(dllPaths[target.ProjectName], gamePath, deploySubdir);
                    }
                }

                if (deployInteropDlls)
                {
                    foreach (var dllName in s_harmonyInteropDlls)
                    {
                        Console.WriteLine($"DEBUG: Deploying interop DLL: {dllName}");
                        DeployInteropDll(repoRoot, dllName, gamePath);
                    }
                }

                // Deploy third-party mods
                foreach (var (modName, dllPath, modDir) in thirdPartyMods)
                {
                    var deploySubdir = $"BaseMods/{modName}";
                    deployedPaths[modName] = DeployDll(dllPath, gamePath, deploySubdir);
                    DeployThirdPartyModContent(modDir, gamePath, deploySubdir);
                }

                Console.WriteLine();
                Console.WriteLine("".PadRight(60, '='));
                Console.WriteLine("Deployment Summary");
                Console.WriteLine("".PadRight(60, '='));

                foreach (var target in targets)
                {
                    var path = deployedPaths[target.ProjectName];
                    Console.WriteLine($"{target.ProjectName}: {path}");
                    Console.WriteLine($"Size: {_fileSystem.GetFileSize(path):N0} bytes");
                }

                foreach (var (modName, _, _) in thirdPartyMods)
                {
                    var path = deployedPaths[modName];
                    Console.WriteLine($"[third-party] {modName}: {path}");
                    Console.WriteLine($"Size: {_fileSystem.GetFileSize(path):N0} bytes");
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
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine($"ERROR: Deployment failed: {ex.Message}");
                return 1;
            }
        }

        private string BuildProject(string projectPath, string configuration, string assemblyName)
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
            var dllName = $"{assemblyName}.dll";
            var dllPath = FindExistingDll(projectDir, configuration, dllName)
                ?? throw new FileNotFoundException($"Built DLL not found for {projectName}");

            Console.WriteLine($"Built: {dllPath}");
            Console.WriteLine($"Size: {_fileSystem.GetFileSize(dllPath):N0} bytes");

            return dllPath;
        }

        private string? FindExistingDll(string projectDir, string configuration, string dllName)
        {
            var projectName = Path.GetFileNameWithoutExtension(dllName);

            // Try bin/net35/{dllName} first (OutputPath=bin\ in Directory.Build.props)
            // This is the canonical output path and takes priority over stale config-specific paths
            var path = Path.Combine(projectDir, "bin", "net35", dllName);
            if (_fileSystem.FileExists(path))
            {
                return path;
            }

            // Try bin/{config}/net35/{projectName}/{dllName}
            path = Path.Combine(projectDir, "bin", configuration, "net35", projectName, dllName);
            if (_fileSystem.FileExists(path))
            {
                return path;
            }

            // Try bin/{config}/net35/{dllName}
            path = Path.Combine(projectDir, "bin", configuration, "net35", dllName);
            if (_fileSystem.FileExists(path))
            {
                return path;
            }

            return null;
        }

        private string DeployDll(string sourceDll, string gamePath, string destSubdir)
        {
            var destDir = GetDeployDestDir(gamePath, destSubdir);
            var destDll = Path.Combine(destDir, Path.GetFileName(sourceDll) ?? string.Empty);

            Console.WriteLine();
            Console.WriteLine($"Deploying {Path.GetFileName(sourceDll)} to {destDir}...");

            // Create destination directory if it doesn't exist
            if (!_fileSystem.DirectoryExists(destDir))
            {
                _fileSystem.CreateDirectory(destDir);
            }

            // Copy the DLL
            _fileSystem.CopyFile(sourceDll, destDll, true);

            // Verify deployment
            if (!_fileSystem.FileExists(destDll))
            {
                throw new IOException($"Failed to copy {Path.GetFileName(sourceDll)} to {destDll}");
            }

            var fileSize = _fileSystem.GetFileSize(destDll);
            if (fileSize == 0)
            {
                throw new IOException($"Deployed DLL is empty: {destDll}");
            }

            Console.WriteLine($"Deployed: {destDll}");
            Console.WriteLine($"Size: {fileSize:N0} bytes");

            return destDll;
        }

        private void DeployModContent(string modDllPath, string gamePath, string destSubdir)
        {
            var sourceDir = Path.GetDirectoryName(modDllPath) ?? string.Empty;
            var destDir = GetDeployDestDir(gamePath, destSubdir);

            // Deploy Common DLL
            var commonDlls = _fileSystem.GetFiles(sourceDir, "Hemocode.Common.*.dll");
            foreach (var commonDll in commonDlls)
            {
                var destCommonDll = Path.Combine(destDir, Path.GetFileName(commonDll));
                _fileSystem.CopyFile(commonDll, destCommonDll, true);
                Console.WriteLine($"Deployed: {Path.GetFileName(commonDll)} -> {destDir}");
            }

            // Deploy content directories (Info/, Assets/, Localize/)
            foreach (var contentDir in s_modContentDirs)
            {
                var sourceContentDir = Path.Combine(sourceDir, contentDir);
                if (_fileSystem.DirectoryExists(sourceContentDir))
                {
                    var destContentDir = Path.Combine(destDir, contentDir);
                    _fileSystem.CopyDirectory(sourceContentDir, destContentDir, true);
                    Console.WriteLine($"Deployed: {contentDir}/ -> {destContentDir}");
                }
            }
        }

        private void DeployInteropDll(string repoRoot, string dllName, string gamePath)
        {
            // Special case: 12Harmony.dll is needed for mods that reference Harmony 1.2 by that name
            // Copy from 0Harmony12.dll since they're the same library
            string sourceDll;
            if (dllName == "12Harmony.dll")
            {
                sourceDll = Path.Combine(repoRoot, "RetargetHarmony", "lib", "0Harmony12.dll");
            }
            else
            {
                sourceDll = Path.Combine(repoRoot, "RetargetHarmony", "lib", dllName);
            }

            if (!_fileSystem.FileExists(sourceDll))
            {
                throw new FileNotFoundException(
                    $"Harmony interop DLL not found: {sourceDll}\n" +
                    $"Expected vendored DLLs from BepInEx/HarmonyInteropDlls in RetargetHarmony/lib/");
            }

            var destPath = DeployDll(sourceDll, gamePath, "core");
            Console.WriteLine($"Interop: {destPath}");
            Console.WriteLine($"Size: {_fileSystem.GetFileSize(destPath):N0} bytes");
        }

        private static string ResolveDeploySubdir(DeploymentTarget target, DeploymentProfile? profile)
        {
            if (profile?.DeployOverrides != null &&
                profile.DeployOverrides.TryGetValue(target.ProjectName, out var overrideSubdir))
            {
                return overrideSubdir;
            }

            return target.DeploySubdir;
        }

        private static string GetDeployDestDir(string gamePath, string destSubdir)
        {
            if (destSubdir.StartsWith("BaseMods", StringComparison.OrdinalIgnoreCase))
            {
                var subFolder = destSubdir.Length > 8 ? destSubdir[9..] : null;
                if (!string.IsNullOrEmpty(subFolder))
                {
                    return Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods", subFolder);
                }

                return Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");
            }

            return Path.Combine(gamePath, "BepInEx", destSubdir);
        }

        private List<(string ModName, string DllPath, string ModDir)> DiscoverThirdPartyMods(string repoRoot)
        {
            var mods = new List<(string ModName, string DllPath, string ModDir)>();
            var thirdPartyDir = Path.Combine(repoRoot, ThirdPartyModsRelativePath);

            if (!_fileSystem.DirectoryExists(thirdPartyDir))
            {
                Console.WriteLine("WARNING: Third-party mods directory not found, skipping: " + thirdPartyDir);
                return mods;
            }

            var modDirs = _fileSystem.GetDirectories(thirdPartyDir, "*");
            foreach (var modDir in modDirs)
            {
                var modName = Path.GetFileName(modDir);
                var dlls = _fileSystem.GetFiles(modDir, "*.dll");

                if (dlls.Length == 0)
                {
                    Console.WriteLine($"WARNING: No DLL found in third-party mod folder '{modName}', skipping.");
                    continue;
                }

                if (dlls.Length > 1)
                {
                    Console.WriteLine($"WARNING: Multiple DLLs found in third-party mod folder '{modName}', skipping.");
                    continue;
                }

                mods.Add((modName, dlls[0], modDir));
                Console.WriteLine($"Discovered third-party mod: {modName} ({Path.GetFileName(dlls[0])})");
            }

            return mods;
        }

        private void DeployThirdPartyModContent(string modDir, string gamePath, string destSubdir)
        {
            var destDir = GetDeployDestDir(gamePath, destSubdir);

            foreach (var contentDir in s_modContentDirs)
            {
                var sourceContentDir = Path.Combine(modDir, contentDir);
                if (_fileSystem.DirectoryExists(sourceContentDir))
                {
                    var destContentDir = Path.Combine(destDir, contentDir);
                    _fileSystem.CopyDirectory(sourceContentDir, destContentDir, true);
                    Console.WriteLine($"Deployed: {contentDir}/ -> {destContentDir}");
                }
            }
        }

        private string? FindRepositoryRoot()
        {
            var currentDir = _fileSystem.GetCurrentDirectory();
            var dir = currentDir;

            while (!string.IsNullOrEmpty(dir))
            {
                if (_fileSystem.DirectoryExists(Path.Combine(dir, ".git")))
                {
                    return dir;
                }

                if (_fileSystem.FileExists(Path.Combine(dir, "LobotomyCorporationMods.sln")))
                {
                    return dir;
                }

                dir = Path.GetDirectoryName(dir);
            }

            return null;
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

        private record DeploymentTarget(string ProjectName, string AssemblyName, string DeploySubdir, bool IsMod, string? ProjectPath = null);

        internal sealed class BuildFailedException : Exception
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
}
