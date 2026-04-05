// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Interfaces.Configuration
{
    /// <summary>
    /// Configuration for a single deploy target.
    /// </summary>
    public class DeployTargetConfig
    {
        /// <summary>
        /// Gets or sets the project name (used for profile target matching).
        /// </summary>
        public string ProjectName { get; set; } = "";

        /// <summary>
        /// Gets or sets the assembly name (output DLL name without extension).
        /// </summary>
        public string AssemblyName { get; set; } = "";

        /// <summary>
        /// Gets or sets the deployment subdirectory (e.g., "BaseMods/Hemocode.Playwright").
        /// </summary>
        public string DeploySubdir { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether this target is a mod (deploys Common DLL and content dirs).
        /// </summary>
        public bool IsMod { get; set; }

        /// <summary>
        /// Gets or sets an optional custom project path relative to the repo root.
        /// </summary>
        public string? ProjectPath { get; set; }
    }
}
