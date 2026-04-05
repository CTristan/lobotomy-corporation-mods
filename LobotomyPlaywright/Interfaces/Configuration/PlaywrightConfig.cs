// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LobotomyPlaywright.Interfaces.Configuration
{
    /// <summary>
    /// Root configuration loaded from playwright.json, containing deploy targets, profiles, and paths.
    /// </summary>
    public class PlaywrightConfig
    {
        /// <summary>
        /// Gets or sets the list of deploy targets available in this repository.
        /// </summary>
        public Collection<DeployTargetConfig> DeployTargets { get; set; } = [];

        /// <summary>
        /// Gets or sets the deployment profiles.
        /// </summary>
        public Dictionary<string, DeploymentProfile> Profiles { get; set; } = [];

        /// <summary>
        /// Gets or sets the relative path to third-party mods directory.
        /// </summary>
        public string? ThirdPartyModsPath { get; set; }

        /// <summary>
        /// Gets or sets the relative path to BepInEx source files.
        /// </summary>
        public string? BepInExSourcePath { get; set; }

        /// <summary>
        /// Gets or sets the list of Harmony interop DLL filenames to deploy to BepInEx/core.
        /// </summary>
        public Collection<string>? HarmonyInteropDlls { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the directory containing Harmony interop DLLs.
        /// </summary>
        public string? HarmonyInteropSourcePath { get; set; }
    }
}
