// SPDX-License-Identifier: MIT

using System;
using System.Collections.ObjectModel;

namespace Harmony2ForLmm.Models
{
    /// <summary>
    /// Represents the installation manifest stored at .harmony2forlmm/manifest.json.
    /// </summary>
    public sealed class InstallationManifest
    {
        /// <summary>
        /// Gets or sets the installed bundle version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp when the installation occurred.
        /// </summary>
        public DateTime InstalledAt { get; set; }

        /// <summary>
        /// Gets or sets the list of installed file paths relative to the game root.
        /// </summary>
        public Collection<string> Files { get; set; } = [];
    }
}
