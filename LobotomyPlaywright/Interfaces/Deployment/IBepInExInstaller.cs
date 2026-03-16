// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Interfaces.Deployment
{
    /// <summary>
    /// Interface for installing BepInEx into the game directory.
    /// </summary>
    public interface IBepInExInstaller
    {
        /// <summary>
        /// Installs BepInEx files into the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation path.</param>
        /// <param name="sourcePath">The directory containing BepInEx files to install.</param>
        void Install(string gamePath, string sourcePath);
    }
}
