// SPDX-License-Identifier: MIT

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Locates the Lobotomy Corporation game installation directory.
    /// </summary>
    public interface IGamePathFinder
    {
        /// <summary>
        /// Attempts to auto-detect the game installation path.
        /// </summary>
        /// <returns>The game path if found, or null.</returns>
        string? FindGamePath();
    }
}
