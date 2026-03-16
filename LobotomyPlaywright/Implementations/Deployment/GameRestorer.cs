// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Restores a game installation to vanilla state using snapshot files.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the GameRestorer class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    public sealed class GameRestorer(IFileSystem fileSystem) : IGameRestorer
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public void RestoreTargeted(string gamePath, string snapshotPath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(snapshotPath);

            var baseModsPath = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");
            var bepInExPath = Path.Combine(gamePath, "BepInEx");
            var managedPath = Path.Combine(gamePath, "LobotomyCorp_Data", "Managed");
            var vanillaManagedPath = Path.Combine(snapshotPath, "LobotomyCorp_vanilla", "LobotomyCorp_Data", "Managed");

            ValidateVanillaSnapshot(vanillaManagedPath, snapshotPath);

            Console.WriteLine("Restoring game to vanilla state (targeted)...");

            if (_fileSystem.DirectoryExists(baseModsPath))
            {
                Console.WriteLine($"Deleting {baseModsPath}");
                _fileSystem.DeleteDirectory(baseModsPath, true);
            }

            if (_fileSystem.DirectoryExists(bepInExPath))
            {
                Console.WriteLine($"Deleting {bepInExPath}");
                _fileSystem.DeleteDirectory(bepInExPath, true);
            }

            if (_fileSystem.DirectoryExists(managedPath))
            {
                Console.WriteLine($"Deleting {managedPath}");
                _fileSystem.DeleteDirectory(managedPath, true);
            }

            Console.WriteLine($"Restoring Managed/ from {vanillaManagedPath}");
            _fileSystem.CopyDirectory(vanillaManagedPath, managedPath, true);

            Console.WriteLine("Targeted restore complete.");
        }

        public void RestoreFull(string gamePath, string snapshotPath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(snapshotPath);

            var vanillaGamePath = Path.Combine(snapshotPath, "LobotomyCorp_vanilla");

            if (!_fileSystem.DirectoryExists(vanillaGamePath))
            {
                throw new DirectoryNotFoundException(
                    $"Vanilla game snapshot not found: {vanillaGamePath}\n" +
                    $"Copy your vanilla game installation to: {vanillaGamePath}");
            }

            Console.WriteLine("Restoring game to vanilla state (full)...");
            Console.WriteLine($"Copying {vanillaGamePath} -> {gamePath}");

            _fileSystem.CopyDirectory(vanillaGamePath, gamePath, true);

            Console.WriteLine("Full restore complete.");
        }

        private void ValidateVanillaSnapshot(string vanillaManagedPath, string snapshotPath)
        {
            if (!_fileSystem.DirectoryExists(vanillaManagedPath))
            {
                var expectedPath = Path.Combine(snapshotPath, "LobotomyCorp_vanilla");
                throw new DirectoryNotFoundException(
                    $"Vanilla game snapshot not found: {vanillaManagedPath}\n" +
                    $"Copy your vanilla Lobotomy Corporation installation to: {expectedPath}\n" +
                    $"The directory should contain LobotomyCorp_Data/Managed/ with the unmodified game DLLs.");
            }
        }
    }
}
