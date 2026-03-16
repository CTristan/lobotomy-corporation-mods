// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Installs LMM (Lobotomy Mod Manager) patch files into the game directory.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the LmmInstaller class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    public sealed class LmmInstaller(IFileSystem fileSystem) : ILmmInstaller
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public void Install(string gamePath, string testdataPath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(testdataPath);

            var lmmSnapshotPath = Path.Combine(testdataPath, "LobotomyCorp_LMM");
            var lmmManagedPath = Path.Combine(lmmSnapshotPath, "LobotomyCorp_Data", "Managed");
            var lmmBaseModsPath = Path.Combine(lmmSnapshotPath, "LobotomyCorp_Data", "BaseMods");
            var gameManagedPath = Path.Combine(gamePath, "LobotomyCorp_Data", "Managed");
            var gameBaseModsPath = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");

            if (!_fileSystem.DirectoryExists(lmmSnapshotPath))
            {
                throw new DirectoryNotFoundException($"LMM snapshot directory not found: {lmmSnapshotPath}");
            }

            Console.WriteLine("Installing LMM...");

            // Overlay LMM Managed/ onto game (overwrites vanilla Assembly-CSharp.dll with patched version, adds LobotomyBaseModLib.dll, etc.)
            Console.WriteLine($"Copying Managed/ from {lmmManagedPath}");
            _fileSystem.CopyDirectory(lmmManagedPath, gameManagedPath, true);

            // Copy BaseMods/ (contains BaseModList_v2.xml and mod loading infrastructure)
            if (_fileSystem.DirectoryExists(lmmBaseModsPath))
            {
                Console.WriteLine($"Copying BaseMods/ from {lmmBaseModsPath}");
                _fileSystem.CopyDirectory(lmmBaseModsPath, gameBaseModsPath, true);
            }

            Console.WriteLine("LMM installation complete.");
        }
    }
}
