// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SetupExternal
{
    /// <summary>
    /// Handles copying game DLLs with SHA256 hash comparison to avoid unnecessary copies.
    /// </summary>
    internal static class FileSyncer
    {
        private const string AssemblyCSharpFileName = "Assembly-CSharp.dll";

        /// <summary>
        /// Writes a debug message if debug logging is enabled.
        /// </summary>
        private static void DebugLog(string message)
        {
            Program.DebugLog($"[FileSyncer] {message}");
        }

        /// <summary>
        /// Result of a file sync operation.
        /// </summary>
        internal sealed class SyncResult
        {
            public int FilesCopied { get; set; }
            public int FilesUpdated { get; set; }
            public int FilesSkipped { get; set; }
            public bool AssemblyCSharpChanged { get; set; }
            public bool LobotomyBaseModLibChanged { get; set; }
        }

        /// <summary>
        /// Synchronizes DLL files from the game's Managed directory to the external directory.
        /// </summary>
        /// <param name="sourcePath">The game installation path.</param>
        /// <param name="destinationPath">The external directory path.</param>
        /// <param name="force">If true, skips hash checking and forcibly copies all files.</param>
        /// <returns>A summary of the sync operation.</returns>
        public static SyncResult SyncDlls(string sourcePath, string destinationPath, bool force = false)
        {
            SyncResult result = new();
            string sourceManagedDir = Path.Combine(sourcePath, "LobotomyCorp_Data", "Managed");
            string destManagedDir = Path.Combine(destinationPath, "LobotomyCorp_Data", "Managed");

            DebugLog($"Starting sync from {sourceManagedDir} to {destManagedDir}");
            if (force)
            {
                DebugLog("Force mode enabled - skipping hash checks");
            }

            if (!Directory.Exists(sourceManagedDir))
            {
                Console.Error.WriteLine($"Source directory not found: {sourceManagedDir}");
                DebugLog($"Source directory not found!");
                return result;
            }

            // Create destination directory if it doesn't exist
            _ = Directory.CreateDirectory(destManagedDir);
            DebugLog($"Destination directory created: {destManagedDir}");

            // Get all DLL files in source
            string[] dllFiles = Directory.GetFiles(sourceManagedDir, "*.dll", SearchOption.TopDirectoryOnly);
            DebugLog($"Found {dllFiles.Length} DLL files in source");

            foreach (string sourceFile in dllFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(destManagedDir, fileName);

                if (!File.Exists(destFile))
                {
                    // New file - copy it
                    File.Copy(sourceFile, destFile, false);
                    result.FilesCopied++;

                    UpdateSyncResult(result, fileName);

                    Console.WriteLine($"Copied: {fileName}");
                    DebugLog($"Copied (new file): {fileName}");
                }
                else
                {
                    if (force)
                    {
                        // Force mode - copy without checking hashes
                        File.Copy(sourceFile, destFile, true);
                        result.FilesUpdated++;

                        UpdateSyncResult(result, fileName);

                        Console.WriteLine($"Updated (force): {fileName}");
                        DebugLog($"Updated (force): {fileName}");
                    }
                    else
                    {
                        // File exists - compare hashes
                        string sourceHash = ComputeFileHash(sourceFile);
                        string destHash = ComputeFileHash(destFile);

                        DebugLog($"Checking {fileName}: source hash = {sourceHash[..8]}..., dest hash = {destHash[..8]}...");

                        if (!sourceHash.Equals(destHash, StringComparison.OrdinalIgnoreCase))
                        {
                            // Hash differs - update the file
                            File.Copy(sourceFile, destFile, true);
                            result.FilesUpdated++;

                            UpdateSyncResult(result, fileName);

                            Console.WriteLine($"Updated: {fileName}");
                            DebugLog($"Updated (hash changed): {fileName}");
                        }
                        else
                        {
                            // Hash matches - skip
                            result.FilesSkipped++;
                            DebugLog($"Skipped (hash match): {fileName}");
                        }
                    }
                }
            }

            return result;
        }

        private static void UpdateSyncResult(SyncResult result, string fileName)
        {
            if (fileName.Equals(AssemblyCSharpFileName, StringComparison.OrdinalIgnoreCase))
            {
                result.AssemblyCSharpChanged = true;
            }
            else if (fileName.Equals("LobotomyBaseModLib.dll", StringComparison.OrdinalIgnoreCase))
            {
                result.LobotomyBaseModLibChanged = true;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        private static string ComputeFileHash(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(filePath);
            byte[] hashBytes = sha256.ComputeHash(stream);
            return ConvertHashToString(hashBytes);
        }

        /// <summary>
        /// Converts a byte array hash to a hexadecimal string.
        /// </summary>
        private static string ConvertHashToString(byte[] hash)
        {
            StringBuilder sb = new();
            foreach (byte b in hash)
            {
                _ = sb.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}
