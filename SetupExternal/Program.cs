// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Linq;

namespace SetupExternal;

#pragma warning disable RCS1102 // Make class static
#pragma warning disable CA1303 // Do not pass literals as localized parameters

/// <summary>
/// SetupExternal - Game file initialization tool for Lobotomy Corporation mod development.
///
/// This tool automatically:
/// 1. Locates the Lobotomy Corporation game installation
/// 2. Copies game DLLs to external/ with hash-based change detection
/// 3. Decompiles Assembly-CSharp.dll and LobotomyBaseModLib.dll for reference
///
/// Usage:
///   dotnet run --project SetupExternal
///   dotnet run --project SetupExternal -- --path "/path/to/LobotomyCorp"
///   dotnet run --project SetupExternal -- --debug
///   dotnet run --project SetupExternal -- --force
/// </summary>
public sealed class Program
{
    private const string ExternalDirectoryName = "external";
    public static bool DebugEnabled { get; private set; }

    /// <summary>
    /// Writes a debug message if debug logging is enabled.
    /// </summary>
    public static void DebugLog(string message)
    {
        if (DebugEnabled)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    public static int Main(string[] args)
    {
        args ??= Array.Empty<string>();
        var (manualPath, debugEnabled, force) = ParseArguments(args);
        DebugEnabled = debugEnabled;

        Console.WriteLine("SetupExternal - Game File Initialization Tool");
        Console.WriteLine();

        if (DebugEnabled)
        {
            Console.WriteLine("[DEBUG] Debug logging enabled");
            Console.WriteLine("[DEBUG] Arguments: " + string.Join(", ", args));
            Console.WriteLine();
        }

        // Parse CLI arguments

        // Get the solution root directory
        var solutionRoot = GetSolutionRootDirectory();
        var externalPath = Path.Combine(solutionRoot, ExternalDirectoryName);

        // Find or use manual game path
        string? gamePath;
        if (manualPath != null)
        {
            gamePath = manualPath;
            Console.WriteLine($"Using manual game path: {gamePath}");
            DebugLog($"Manual path provided: {gamePath}");
        }
        else
        {
            Console.WriteLine("Searching for Lobotomy Corporation installation...");
            DebugLog("Starting automatic game path search");
            gamePath = GamePathFinder.FindGamePath();

            if (gamePath == null)
            {
                Console.Error.WriteLine("Game installation not found automatically.");
                PrintNotFoundHelp();
                return 1;
            }

            Console.WriteLine($"Found game installation: {gamePath}");
            DebugLog($"Found game at: {gamePath}");
        }

        Console.WriteLine();

        // Validate the game path
        if (!ValidateGamePath(gamePath))
        {
            Console.Error.WriteLine($"Invalid game path: {gamePath}");
            Console.Error.WriteLine("The path must contain LobotomyCorp_Data/Managed/Assembly-CSharp.dll");
            return 1;
        }

        Console.WriteLine($"Syncing DLLs to: {externalPath}");
        DebugLog($"Syncing from game path: {gamePath}");
        DebugLog($"Syncing to external path: {externalPath}");
        if (force)
        {
            Console.WriteLine("Force mode enabled - all files will be copied regardless of hash");
            DebugLog("Force mode enabled");
        }
        Console.WriteLine();

        // Sync DLL files
        var syncResult = FileSyncer.SyncDlls(gamePath, externalPath, force);

        // Retarget all DLLs to fix mscorlib version mismatches
        Console.WriteLine("Checking DLLs for mscorlib version mismatches...");
        var destManagedDir = Path.Combine(externalPath, "LobotomyCorp_Data", "Managed");

        var dllFiles = Directory.GetFiles(destManagedDir, "*.dll");
        int retargetedCount = 0;

        foreach (var dllPath in dllFiles)
        {
            if (AssemblyRetargeter.Retarget(dllPath))
            {
                retargetedCount++;
                var dllName = Path.GetFileName(dllPath);
                Console.WriteLine($"  Retargeted {dllName} to mscorlib v2.0.0.0");

                // Track specific DLLs for decompilation
                if (dllName.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase))
                {
                    syncResult.AssemblyCSharpChanged = true;
                }
                else if (dllName.Equals("LobotomyBaseModLib.dll", StringComparison.OrdinalIgnoreCase))
                {
                    syncResult.LobotomyBaseModLibChanged = true;
                }
            }
        }

        if (retargetedCount > 0)
        {
            Console.WriteLine($"  Retargeted {retargetedCount} DLL(s) total");
        }
        else
        {
            Console.WriteLine("  No DLLs needed retargeting");
        }

        // Print sync summary
        Console.WriteLine();
        Console.WriteLine("Sync Summary:");
        Console.WriteLine($"  Files copied: {syncResult.FilesCopied}");
        Console.WriteLine($"  Files updated: {syncResult.FilesUpdated}");
        Console.WriteLine($"  Files skipped: {syncResult.FilesSkipped}");
        DebugLog($"Assembly-CSharp.dll changed: {syncResult.AssemblyCSharpChanged}");
        DebugLog($"LobotomyBaseModLib.dll changed: {syncResult.LobotomyBaseModLibChanged}");

        // Decompile if Assembly-CSharp.dll changed
        if (syncResult.AssemblyCSharpChanged)
        {
            Console.WriteLine();
            Console.WriteLine("Assembly-CSharp.dll was changed - decompiling...");
            Console.WriteLine();
            DebugLog("Starting decompilation of Assembly-CSharp.dll");

            var success = Decompiler.DecompileDll(externalPath, "Assembly-CSharp.dll");

            if (!success)
            {
                Console.Error.WriteLine("Decompilation of Assembly-CSharp.dll failed. You may need to run 'dotnet tool restore' manually.");
                DebugLog("Decompilation of Assembly-CSharp.dll failed");
                return 1;
            }

            Console.WriteLine("Decompilation of Assembly-CSharp.dll completed.");
            DebugLog("Decompilation of Assembly-CSharp.dll completed successfully");
        }

        // Decompile if LobotomyBaseModLib.dll changed
        if (syncResult.LobotomyBaseModLibChanged)
        {
            Console.WriteLine();
            Console.WriteLine("LobotomyBaseModLib.dll was changed - decompiling...");
            Console.WriteLine();
            DebugLog("Starting decompilation of LobotomyBaseModLib.dll");

            var success = Decompiler.DecompileDll(externalPath, "LobotomyBaseModLib.dll");

            if (!success)
            {
                Console.Error.WriteLine("Decompilation of LobotomyBaseModLib.dll failed. You may need to run 'dotnet tool restore' manually.");
                DebugLog("Decompilation of LobotomyBaseModLib.dll failed");
                return 1;
            }

            Console.WriteLine("Decompilation of LobotomyBaseModLib.dll completed.");
            DebugLog("Decompilation of LobotomyBaseModLib.dll completed successfully");
        }

        if (syncResult.FilesCopied == 0 && syncResult.FilesUpdated == 0)
        {
            Console.WriteLine();
            Console.WriteLine("No files were changed. Skipping decompilation.");
            DebugLog("No changes detected, skipping decompilation");
        }

        Console.WriteLine();
        Console.WriteLine("SetupExternal completed successfully.");
        Console.WriteLine($"You can now build the solution with: dotnet build LobotomyCorporationMods.sln");
        DebugLog("SetupExternal completed successfully");

        return 0;
    }

    /// <summary>
    /// Parses CLI arguments for a manual game path and debug flag.
    /// </summary>
    public static (string? Path, bool Debug, bool Force) ParseArguments(string[] args)
    {
        args ??= Array.Empty<string>();
        string? path = null;
        bool debug = false;
        bool force = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--path", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                path ??= args[i + 1];
                i++; // Skip the next argument as it's the path value
            }
            else if (args[i].Equals("--debug", StringComparison.OrdinalIgnoreCase) ||
                     args[i].Equals("-d", StringComparison.OrdinalIgnoreCase) ||
                     args[i].Equals("/d", StringComparison.OrdinalIgnoreCase))
            {
                debug = true;
            }
            else if (args[i].Equals("--force", StringComparison.OrdinalIgnoreCase) ||
                     args[i].Equals("-f", StringComparison.OrdinalIgnoreCase))
            {
                force = true;
            }
        }

        return (path, debug, force);
    }

    /// <summary>
    /// Gets the solution root directory (where .sln file is located).
    /// </summary>
    private static string GetSolutionRootDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Search upward for the .sln file
        while (currentDir != null)
        {
            string[] slnFiles;
            try
            {
                slnFiles = Directory.GetFiles(currentDir, "*.sln");
            }
            catch (UnauthorizedAccessException)
            {
                break;
            }
            catch (DirectoryNotFoundException)
            {
                break;
            }

            if (slnFiles.Any(f => Path.GetFileName(f).Equals("LobotomyCorporationMods.sln", StringComparison.OrdinalIgnoreCase)))
            {
                return currentDir;
            }

            try
            {
                var parentDir = Directory.GetParent(currentDir);
                currentDir = parentDir?.FullName;
            }
            catch (UnauthorizedAccessException)
            {
                break;
            }
            catch (DirectoryNotFoundException)
            {
                break;
            }
        }

        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Validates that a path is a valid Lobotomy Corporation installation.
    /// </summary>
    private static bool ValidateGamePath(string path)
    {
        var assemblyCSharpPath = Path.Combine(path, "LobotomyCorp_Data", "Managed", "Assembly-CSharp.dll");
        return File.Exists(assemblyCSharpPath);
    }

    /// <summary>
    /// Prints helpful information when the game is not found automatically.
    /// </summary>
    private static void PrintNotFoundHelp()
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("Expected locations searched:");
        Console.Error.WriteLine("  Windows:");
        Console.Error.WriteLine("    C:\\Program Files (x86)\\Steam\\steamapps\\common\\LobotomyCorp");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  Linux:");
        Console.Error.WriteLine("    ~/.steam/steam/steamapps/common/LobotomyCorp");
        Console.Error.WriteLine("    ~/.local/share/Steam/steamapps/common/LobotomyCorp");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  macOS (CrossOver): CrossOver bottles AND external Steam libraries (mounted volumes like `/Volumes/*`)");
        Console.Error.WriteLine();
        Console.Error.WriteLine("To specify a manual path, use:");
        Console.Error.WriteLine("  dotnet run --project SetupExternal -- --path \"/path/to/LobotomyCorp\"");
        Console.Error.WriteLine();
    }
}
