// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LobotomyPlaywright.Interfaces.Configuration
{
    /// <summary>
    /// Defines a deployment profile specifying which components to install and deploy.
    /// </summary>
    public class DeploymentProfile
    {
        /// <summary>
        /// Gets or sets the collection of project names to build and deploy.
        /// </summary>
        public Collection<string> DeployTargets { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether to install LMM (Lobotomy Mod Manager).
        /// </summary>
        public bool InstallLmm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to install BepInEx framework.
        /// </summary>
        public bool InstallModLoader { get; set; }

        /// <summary>
        /// Gets or sets optional deploy path overrides per project name.
        /// Maps project name to alternate deploy subdirectory (e.g., "plugins/LobotomyPlaywright" for BepInEx-only).
        /// </summary>
        public Dictionary<string, string>? DeployOverrides { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include third-party mods from external/thirdparty-mods/.
        /// </summary>
        public bool IncludeThirdPartyMods { get; set; }
    }
}
