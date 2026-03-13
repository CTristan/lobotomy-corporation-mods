// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace RetargetHarmony.Installer.Interfaces
{
    /// <summary>
    /// Installs BepInEx 5 and RetargetHarmony into a Lobotomy Corporation game directory.
    /// </summary>
    public interface IInstallerService
    {
        /// <summary>
        /// Checks whether BepInEx is already installed in the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>True if BepInEx is detected.</returns>
        bool IsBepInExInstalled(string gamePath);

        /// <summary>
        /// Checks whether RetargetHarmony is already installed in the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>True if RetargetHarmony is detected.</returns>
        bool IsRetargetHarmonyInstalled(string gamePath);

        /// <summary>
        /// Installs BepInEx 5 and RetargetHarmony to the game directory.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>The result of the installation operation.</returns>
        InstallResult Install(string gamePath);
    }

    /// <summary>
    /// Result of an install operation.
    /// </summary>
    public sealed class InstallResult
    {
        /// <summary>
        /// Gets a value indicating whether the installation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a list of files that were created or overwritten.
        /// </summary>
        public IReadOnlyList<string> FilesWritten { get; }

        /// <summary>
        /// Gets an error message if the installation failed.
        /// </summary>
        public string? ErrorMessage { get; }

        private InstallResult(bool isSuccess, IReadOnlyList<string> filesWritten, string? errorMessage)
        {
            IsSuccess = isSuccess;
            FilesWritten = filesWritten;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful install result.
        /// </summary>
        public static InstallResult Success(IReadOnlyList<string> filesWritten)
        {
            return new InstallResult(true, filesWritten, null);
        }

        /// <summary>
        /// Creates a failed install result.
        /// </summary>
        public static InstallResult Failure(string errorMessage)
        {
            return new InstallResult(false, [], errorMessage);
        }
    }
}
