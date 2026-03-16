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
        /// <param name="snapshotPath">The snapshot directory path containing LobotomyCorp_vanilla/.</param>
        void RestoreTargeted(string gamePath, string snapshotPath);

        /// <summary>
        /// Performs a full restore, syncing the entire game directory from the vanilla snapshot.
        /// </summary>
        /// <param name="gamePath">The game installation path.</param>
        /// <param name="snapshotPath">The snapshot directory path containing LobotomyCorp_vanilla/.</param>
        void RestoreFull(string gamePath, string snapshotPath);
    }
}
