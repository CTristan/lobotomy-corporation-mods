// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Interfaces.Deployment
{
    /// <summary>
    /// Interface for installing LMM (Lobotomy Mod Manager) patch files into the game.
    /// </summary>
    public interface ILmmInstaller
    {
        /// <summary>
        /// Installs LMM patch files into the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation path.</param>
        /// <param name="lmmSourcePath">The root directory of the LobotomyModManager tool installation.</param>
        void Install(string gamePath, string lmmSourcePath);
    }
}
