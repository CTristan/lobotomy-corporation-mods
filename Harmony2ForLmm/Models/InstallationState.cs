// SPDX-License-Identifier: MIT

namespace Harmony2ForLmm.Models
{
    /// <summary>
    /// Represents the detected installation state.
    /// </summary>
    public enum InstallationState
    {
        /// <summary>
        /// No manifest found — fresh installation target.
        /// </summary>
        Fresh,

        /// <summary>
        /// Installed version matches the installer version and all files are present.
        /// </summary>
        Current,

        /// <summary>
        /// Installed version is older than the installer version.
        /// </summary>
        Outdated,

        /// <summary>
        /// Installed version is newer than the installer version.
        /// </summary>
        Newer,

        /// <summary>
        /// Manifest exists but some installed files are missing.
        /// </summary>
        Corrupted,
    }
}
