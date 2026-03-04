using System;
using System.Diagnostics;
using System.IO;

namespace SetupExternal;

#pragma warning disable CA1303 // Do not pass literals as localized parameters

/// <summary>
/// Wraps ilspycmd invocation to decompile Assembly-CSharp.dll.
/// </summary>
internal static class Decompiler
{
    private const string DecompiledDirectoryName = "decompiled";
    private static readonly string[] LineSeparators = { "\r", "\n" };

    /// <summary>
    /// Writes a debug message if debug logging is enabled.
    /// </summary>
    private static void DebugLog(string message)
    {
        Program.DebugLog($"[Decompiler] {message}");
    }

    /// <summary>
    /// Decompiles a DLL to a subfolder in the external/decompiled directory.
    /// </summary>
    /// <param name="externalPath">The external directory path.</param>
    /// <param name="dllName">The name of the DLL to decompile (e.g., "Assembly-CSharp.dll").</param>
    /// <returns>True if decompilation succeeded, false otherwise.</returns>
    public static bool DecompileDll(string externalPath, string dllName)
    {
        var dllPath = Path.Combine(
            externalPath,
            "LobotomyCorp_Data",
            "Managed",
            dllName
        );

        DebugLog($"Looking for {dllName} at: {dllPath}");

        if (!File.Exists(dllPath))
        {
            Console.Error.WriteLine($"{dllName} not found at: {dllPath}");
            DebugLog($"{dllName} not found!");
            return false;
        }

        var folderName = Path.GetFileNameWithoutExtension(dllName);
        var decompiledDir = Path.Combine(externalPath, DecompiledDirectoryName, folderName);
        DebugLog($"Decompilation target directory: {decompiledDir}");

        // Clear existing decompiled directory if it exists
        if (Directory.Exists(decompiledDir))
        {
            try
            {
                Directory.Delete(decompiledDir, true);
                Console.WriteLine($"Cleared existing decompiled directory: {decompiledDir}");
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"Failed to clear decompiled directory: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Failed to clear decompiled directory: {ex.Message}");
                return false;
            }
        }

        // Create decompiled directory
        Directory.CreateDirectory(decompiledDir);

        // Ensure dotnet tools are restored
        DebugLog("Ensuring dotnet tools are restored...");
        if (!RestoreDotnetTools())
        {
            Console.Error.WriteLine("Failed to restore dotnet tools");
            DebugLog("dotnet tool restore failed");
            return false;
        }

        // Run ilspycmd
        return RunIlspycmd(dllPath, decompiledDir);
    }

    /// <summary>
    /// Runs dotnet tool restore to ensure ilspycmd is available.
    /// </summary>
    private static bool RestoreDotnetTools()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "tool restore",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Console.WriteLine("Restoring dotnet tools...");

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return false;
        }

        process.WaitForExit();
        var exitCode = process.ExitCode;

        DebugLog($"dotnet tool restore exit code: {exitCode}");

        if (exitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            Console.Error.WriteLine($"dotnet tool restore failed: {error}");
            DebugLog($"dotnet tool restore error: {error}");
            return false;
        }

        DebugLog("dotnet tool restore succeeded");
        return true;
    }

    /// <summary>
    /// Runs ilspycmd to decompile the assembly.
    /// </summary>
    private static bool RunIlspycmd(string dllPath, string outputDir)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"ilspycmd \"{dllPath}\" -p -o \"{outputDir}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        Console.WriteLine($"Decompiling {Path.GetFileName(dllPath)} to {outputDir}...");
        DebugLog($"Running: dotnet ilspycmd \"{dllPath}\" -p -o \"{outputDir}\"");

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return false;
        }

        // Read output
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();
        var exitCode = process.ExitCode;

        if (exitCode != 0)
        {
            Console.Error.WriteLine($"ilspycmd failed with exit code {exitCode}");
            Console.Error.WriteLine($"Error: {error}");
            DebugLog($"ilspycmd exit code: {exitCode}");
            DebugLog($"ilspycmd error: {error}");
            return false;
        }

        Console.WriteLine("Decompilation completed successfully.");
        DebugLog("ilspycmd completed successfully");

        // Print summary if available
        if (!string.IsNullOrWhiteSpace(output))
        {
            var lines = output.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("files", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("classes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  {line.Trim()}");
                }
            }
        }

        return true;
    }
}
