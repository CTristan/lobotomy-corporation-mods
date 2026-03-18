// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Uninstalls BepInEx 5 and RetargetHarmony from a Lobotomy Corporation game directory.
    /// </summary>
    public interface IUninstallerService
    {
        /// <summary>
        /// Uninstalls RetargetHarmony, BepInEx, and optionally flagged BaseMods.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <param name="removeBaseMods">
        /// If true, also removes BaseMods flagged as depending on BepInEx/Harmony 2.
        /// </param>
        /// <returns>The result of the uninstall operation.</returns>
        UninstallResult Uninstall(string gamePath, bool removeBaseMods);
    }

    /// <summary>
    /// Result of an uninstall operation.
    /// </summary>
    public sealed class UninstallResult
    {
        /// <summary>
        /// Gets a value indicating whether the uninstallation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a list of files that were removed.
        /// </summary>
        public IReadOnlyList<string> FilesRemoved { get; }

        /// <summary>
        /// Gets a list of directories that were removed.
        /// </summary>
        public IReadOnlyList<string> DirectoriesRemoved { get; }

        /// <summary>
        /// Gets an error message if the uninstallation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        private UninstallResult(bool isSuccess, IReadOnlyList<string> filesRemoved, IReadOnlyList<string> directoriesRemoved, string? errorMessage)
        {
            IsSuccess = isSuccess;
            FilesRemoved = filesRemoved;
            DirectoriesRemoved = directoriesRemoved;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful uninstall result.
        /// </summary>
        public static UninstallResult Success(IReadOnlyList<string> filesRemoved, IReadOnlyList<string> directoriesRemoved)
        {
            return new UninstallResult(true, filesRemoved, directoriesRemoved, null);
        }

        /// <summary>
        /// Creates a failed uninstall result.
        /// </summary>
        public static UninstallResult Failure(string errorMessage)
        {
            return new UninstallResult(false, [], [], errorMessage);
        }
    }
}
