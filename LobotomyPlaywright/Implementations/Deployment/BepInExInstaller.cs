// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Installs BepInEx files into the game directory.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the BepInExInstaller class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    public sealed class BepInExInstaller(IFileSystem fileSystem) : IBepInExInstaller
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public void Install(string gamePath, string sourcePath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(sourcePath);

            if (!_fileSystem.DirectoryExists(sourcePath))
            {
                throw new DirectoryNotFoundException($"BepInEx source directory not found: {sourcePath}");
            }

            Console.WriteLine("Installing BepInEx...");

            // Copy BepInEx directory
            var bepInExDir = Path.Combine(sourcePath, "BepInEx");
            if (_fileSystem.DirectoryExists(bepInExDir))
            {
                var destBepInExDir = Path.Combine(gamePath, "BepInEx");
                Console.WriteLine($"Copying BepInEx/ -> {destBepInExDir}");
                _fileSystem.CopyDirectory(bepInExDir, destBepInExDir, true);
            }

            // Copy root-level BepInEx files (doorstop_config.ini, winhttp.dll)
            var doorstopConfig = Path.Combine(sourcePath, "doorstop_config.ini");
            if (_fileSystem.FileExists(doorstopConfig))
            {
                var destDoorstop = Path.Combine(gamePath, "doorstop_config.ini");
                Console.WriteLine($"Copying doorstop_config.ini -> {destDoorstop}");
                _fileSystem.CopyFile(doorstopConfig, destDoorstop, true);
            }

            var winhttpDll = Path.Combine(sourcePath, "winhttp.dll");
            if (_fileSystem.FileExists(winhttpDll))
            {
                var destWinhttp = Path.Combine(gamePath, "winhttp.dll");
                Console.WriteLine($"Copying winhttp.dll -> {destWinhttp}");
                _fileSystem.CopyFile(winhttpDll, destWinhttp, true);
            }

            Console.WriteLine("BepInEx installation complete.");
        }
    }
}
