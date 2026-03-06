// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;
using System.IO;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.System;

/// <summary>
/// Implementation of IProcessRunner using System.Diagnostics.Process.
/// </summary>
internal sealed class ProcessRunner : IProcessRunner
{
    public int Run(string fileName, string arguments, string? workingDirectory = null, Func<string?, bool>? outputFilter = null)
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
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode;
        }
        catch (global::System.ComponentModel.Win32Exception)
        {
            return 1;
        }
        catch (FileNotFoundException)
        {
            return 1;
        }
        catch (InvalidOperationException)
        {
            return 1;
        }
    }
}
