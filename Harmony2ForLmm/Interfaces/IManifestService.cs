// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Harmony2ForLmm.Models;

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Reads and writes the installation manifest at .harmony2forlmm/manifest.json.
    /// </summary>
    public interface IManifestService
    {
        /// <summary>
        /// The name of the manifest directory created at the game root.
        /// </summary>
        const string ManifestDirectory = ".harmony2forlmm";

        /// <summary>
        /// The name of the manifest file within the manifest directory.
        /// </summary>
        const string ManifestFileName = "manifest.json";

        /// <summary>
        /// Reads the installation manifest from the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>The manifest, or null if missing or corrupt.</returns>
        InstallationManifest? ReadManifest(string gamePath);

        /// <summary>
        /// Writes the installation manifest to the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <param name="installedFiles">Absolute paths of installed files.</param>
        void WriteManifest(string gamePath, IReadOnlyList<string> installedFiles);

        /// <summary>
        /// Deletes the manifest directory from the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        void DeleteManifest(string gamePath);
    }
}
