// SPDX-License-Identifier: MIT

using System;

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
    /// Gets files matching a pattern in a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="searchPattern">The search pattern (e.g., "*.log").</param>
    /// <returns>The array of file paths.</returns>
    string[] GetFiles(string path, string searchPattern);

    /// <summary>
    /// Gets the last write time of a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The last write time.</returns>
    DateTime GetLastWriteTime(string path);

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file size in bytes.</returns>
    long GetFileSize(string path);

    /// <summary>
    /// Gets the current working directory.
    /// </summary>
    /// <returns>The current working directory.</returns>
    string GetCurrentDirectory();

    /// <summary>
    /// Gets directories matching a pattern.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <returns>The array of directory paths.</returns>
    string[] GetDirectories(string path, string searchPattern);

    /// <summary>
    /// Copies a file.
    /// </summary>
    /// <param name="sourceFileName">The source file path.</param>
    /// <param name="destFileName">The destination file path.</param>
    /// <param name="overwrite">True to overwrite the destination file.</param>
    void CopyFile(string sourceFileName, string destFileName, bool overwrite);

    /// <summary>
    /// Sets a file as executable on Unix systems.
    /// </summary>
    /// <param name="path">The file path.</param>
    void SetFileExecutable(string path);

    /// <summary>
    /// Reads all bytes from a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>The file contents as a byte array.</returns>
    byte[] ReadAllBytes(string path);
}
