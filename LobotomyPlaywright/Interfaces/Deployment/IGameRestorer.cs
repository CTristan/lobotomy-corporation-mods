// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Interfaces.Deployment
{
    /// <summary>
    /// Interface for restoring a game installation to vanilla state.
    /// </summary>
    public interface IGameRestorer
    {
        /// <summary>
        /// Performs a targeted restore, cleaning only mod-affected directories (Managed/, BaseMods/, BepInEx/).
        /// </summary>
        /// <param name="gamePath">The game installation path.</param>
        /// <param name="testdataPath">The testdata directory path.</param>
        void RestoreTargeted(string gamePath, string testdataPath);

        /// <summary>
        /// Performs a full restore, syncing the entire game directory from testdata.
        /// </summary>
        /// <param name="gamePath">The game installation path.</param>
        /// <param name="testdataPath">The testdata directory path.</param>
        void RestoreFull(string gamePath, string testdataPath);
    }
}
