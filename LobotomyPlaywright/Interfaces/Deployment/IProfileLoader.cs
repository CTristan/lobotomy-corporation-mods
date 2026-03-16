// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using LobotomyPlaywright.Interfaces.Configuration;

namespace LobotomyPlaywright.Interfaces.Deployment
{
    /// <summary>
    /// Interface for loading deployment profiles from an external file.
    /// </summary>
    public interface IProfileLoader
    {
        /// <summary>
        /// Loads deployment profiles from the profiles file.
        /// </summary>
        /// <returns>A dictionary of profile name to deployment profile.</returns>
        Dictionary<string, DeploymentProfile> Load();
    }
}
