// SPDX-License-Identifier: MIT

namespace RetargetHarmony.Installer.Interfaces
{
    /// <summary>
    /// Validates that a directory is a Lobotomy Corporation game installation.
    /// </summary>
    public interface IGameDirectoryValidator
    {
        /// <summary>
        /// Validates a path as a Lobotomy Corporation installation directory.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>A validation result indicating success or the reason for failure.</returns>
        GameDirectoryValidationResult Validate(string path);
    }

    /// <summary>
    /// Result of a game directory validation.
    /// </summary>
    public sealed class GameDirectoryValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the path is a valid game installation.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets an error message if the path is not valid.
        /// </summary>
        public string? ErrorMessage { get; }

        private GameDirectoryValidationResult(bool isValid, string? errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static GameDirectoryValidationResult Success()
        {
            return new GameDirectoryValidationResult(true, null);
        }

        /// <summary>
        /// Creates a failed validation result with the given error message.
        /// </summary>
        public static GameDirectoryValidationResult Failure(string errorMessage)
        {
            return new GameDirectoryValidationResult(false, errorMessage);
        }
    }
}
