// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace Harmony2ForLmm.Models
{
    /// <summary>
    /// Result of an installation state detection.
    /// </summary>
    public sealed class InstallationStateResult
    {
        /// <summary>
        /// Gets the detected installation state.
        /// </summary>
        public InstallationState State { get; }

        /// <summary>
        /// Gets the installed version, or null if no manifest was found.
        /// </summary>
        public string? InstalledVersion { get; }

        /// <summary>
        /// Gets the installer's bundle version.
        /// </summary>
        public string InstallerVersion { get; }

        /// <summary>
        /// Gets the list of missing files (for Corrupted state).
        /// </summary>
        public IReadOnlyList<string> MissingFiles { get; }

        private InstallationStateResult(InstallationState state, string? installedVersion, string installerVersion, IReadOnlyList<string> missingFiles)
        {
            State = state;
            InstalledVersion = installedVersion;
            InstallerVersion = installerVersion;
            MissingFiles = missingFiles;
        }

        /// <summary>
        /// Creates a Fresh state result.
        /// </summary>
        public static InstallationStateResult Fresh(string installerVersion)
        {
            return new InstallationStateResult(InstallationState.Fresh, null, installerVersion, []);
        }

        /// <summary>
        /// Creates a result with the given state and version info.
        /// </summary>
        public static InstallationStateResult WithState(InstallationState state, string installedVersion, string installerVersion)
        {
            return new InstallationStateResult(state, installedVersion, installerVersion, []);
        }

        /// <summary>
        /// Creates a Corrupted state result with the list of missing files.
        /// </summary>
        public static InstallationStateResult Corrupted(string installedVersion, string installerVersion, IReadOnlyList<string> missingFiles)
        {
            return new InstallationStateResult(InstallationState.Corrupted, installedVersion, installerVersion, missingFiles);
        }
    }
}
