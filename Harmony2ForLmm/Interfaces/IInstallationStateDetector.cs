// SPDX-License-Identifier: MIT

using Harmony2ForLmm.Models;

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Detects the current installation state by reading the manifest and verifying files.
    /// </summary>
    public interface IInstallationStateDetector
    {
        /// <summary>
        /// Detects the installation state for the given game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>The detected state with version and file information.</returns>
        InstallationStateResult Detect(string gamePath);
    }
}
