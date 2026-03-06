// SPDX-License-Identifier: MIT

namespace LobotomyPlaywright.Interfaces.System;

/// <summary>
/// Interface for file system operations.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Writes all text to a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="contents">The content to write.</param>
    void WriteAllText(string path, string contents);

    /// <summary>
    /// Reads all text from a file, or null if the file doesn't exist.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file contents, or null.</returns>
    string? ReadAllText(string path);

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>True if the directory exists.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory if it doesn't exist.
    /// </summary>
    /// <param name="path">The directory path.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>True if the file exists.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Sets a file as executable on Unix systems.
    /// </summary>
    /// <param name="path">The file path.</param>
    void SetFileExecutable(string path);
}
