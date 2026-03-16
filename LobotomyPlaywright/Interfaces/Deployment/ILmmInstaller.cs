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
        /// <param name="testdataPath">The testdata directory path.</param>
        void Install(string gamePath, string testdataPath);
    }
}
