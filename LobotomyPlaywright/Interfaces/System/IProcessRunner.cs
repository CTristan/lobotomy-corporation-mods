// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics;

namespace LobotomyPlaywright.Interfaces.System;

/// <summary>
/// Interface for running external processes.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a process and returns the exit code.
    /// </summary>
    /// <param name="fileName">The executable file name.</param>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="workingDirectory">Working directory (optional).</param>
    /// <param name="outputFilter">Optional filter for output lines.</param>
    /// <returns>The process exit code.</returns>
    int Run(string fileName, string arguments, string? workingDirectory = null, Func<string?, bool>? outputFilter = null);
}
