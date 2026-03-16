// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Restores a game installation to vanilla state using testdata snapshots.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the GameRestorer class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    public sealed class GameRestorer(IFileSystem fileSystem) : IGameRestorer
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public void RestoreTargeted(string gamePath, string testdataPath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(testdataPath);

            var baseModsPath = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");
            var bepInExPath = Path.Combine(gamePath, "BepInEx");
            var managedPath = Path.Combine(gamePath, "LobotomyCorp_Data", "Managed");
            var testdataManagedPath = Path.Combine(testdataPath, "LobotomyCorp_vanilla", "LobotomyCorp_Data", "Managed");

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

            Console.WriteLine($"Restoring Managed/ from {testdataManagedPath}");
            _fileSystem.CopyDirectory(testdataManagedPath, managedPath, true);

            Console.WriteLine("Targeted restore complete.");
        }

        public void RestoreFull(string gamePath, string testdataPath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(testdataPath);

            var testdataGamePath = Path.Combine(testdataPath, "LobotomyCorp_vanilla");

            Console.WriteLine("Restoring game to vanilla state (full)...");
            Console.WriteLine($"Copying {testdataGamePath} -> {gamePath}");

            _fileSystem.CopyDirectory(testdataGamePath, gamePath, true);

            Console.WriteLine("Full restore complete.");
        }
    }
}
