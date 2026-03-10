// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Infrastructure
{
    /// <summary>
    /// Manages detection and control of game processes.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of ProcessManager class.
    /// </remarks>
    /// <param name="processRunner">The process runner implementation.</param>
    public class ProcessManager(IProcessRunner processRunner)
    {
        private readonly IProcessRunner _processRunner = processRunner;

        /// <summary>
        /// Initializes a new instance of ProcessManager class with default process runner.
        /// </summary>
        public ProcessManager()
            : this(new ProcessRunner())
        {
        }

        /// <summary>
        /// Finds all Lobotomy Corporation game processes.
        /// </summary>
        /// <returns>List of process IDs.</returns>
        public virtual List<int> FindGameProcesses()
        {
            var pids = new List<int>();

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // macOS: Use pgrep to find Wine/CrossOver processes running LobotomyCorp.exe
                    var result = _processRunner.Run("pgrep", "-f LobotomyCorp.exe", null, outputFilter: null);
                    if (result == 0)
                    {
                        // pgrep succeeded, need to get the output
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "pgrep",
                                Arguments = "-f LobotomyCorp.exe",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        _ = process.Start();
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (int.TryParse(line.Trim(), out var pid))
                            {
                                pids.Add(pid);
                            }
                        }
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows: Use GetProcessesByName
                    var processes = Process.GetProcessesByName("LobotomyCorp");
                    pids.AddRange(processes.Select(p => p.Id));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Linux: Use pgrep
                    var result = _processRunner.Run("pgrep", "-f LobotomyCorp.exe", null, outputFilter: null);
                    if (result == 0)
                    {
                        using var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "pgrep",
                                Arguments = "-f LobotomyCorp.exe",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                            }
                        };
                        _ = process.Start();
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (int.TryParse(line.Trim(), out var pid))
                            {
                                pids.Add(pid);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors during process detection
            }

            return pids;
        }

        /// <summary>
        /// Kills the specified processes.
        /// </summary>
        /// <param name="pids">List of process IDs to kill.</param>
        /// <param name="force">Whether to force kill (SIGKILL) instead of graceful termination (SIGTERM).</param>
        /// <returns>True if all processes were killed.</returns>
        public virtual bool KillProcesses(List<int> pids, bool force = false)
        {
            ArgumentNullException.ThrowIfNull(pids);

            var success = true;

            foreach (var pid in pids)
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var process = Process.GetProcessById(pid);
                        process.Kill();
                    }
                    else
                    {
                        // Unix-like systems
                        var signal = force ? 9 : 15; // SIGKILL = 9, SIGTERM = 15
                        _ = _processRunner.Run("kill", $"-{signal} {pid}", null, null);
                    }
                }
                catch (Exception)
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Checks if a process exists.
        /// </summary>
        /// <param name="pid">Process ID to check.</param>
        /// <returns>True if the process exists.</returns>
        public bool ProcessExists(int pid)
        {
            try
            {
                _ = Process.GetProcessById(pid);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
