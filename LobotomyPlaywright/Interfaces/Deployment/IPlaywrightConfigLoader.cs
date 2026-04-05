// SPDX-License-Identifier: MIT

using LobotomyPlaywright.Interfaces.Configuration;

namespace LobotomyPlaywright.Interfaces.Deployment
{
    /// <summary>
    /// Interface for loading the full playwright configuration (targets, profiles, paths).
    /// </summary>
    public interface IPlaywrightConfigLoader
    {
        /// <summary>
        /// Loads the playwright configuration from the config file.
        /// </summary>
        /// <returns>The full playwright configuration.</returns>
        PlaywrightConfig Load();
    }
}
