// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Installs LMM (Lobotomy Mod Manager) patch files into the game directory.
    /// Extracts the necessary files from the LMM tool distribution's PatchFiles directory.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the LmmInstaller class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    public sealed class LmmInstaller(IFileSystem fileSystem) : ILmmInstaller
    {
        private const string PatchFilesSubdir = "LobotomyModManager_Data/PatchFiles";
        private const string PatchedAssemblyName = "Assembly-CSharp_patched.dll";
        private const string GameAssemblyName = "Assembly-CSharp.dll";
        private const string BaseModListXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<ModListXml xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n  <list />\n</ModListXml>";

        private static readonly string[] s_excludedFiles = { "Assembly-CSharp.dll", "Assembly-CSharp_patched.dll", "Lobotomypatch.dll" };

        private readonly IFileSystem _fileSystem = fileSystem;

        public void Install(string gamePath, string lmmSourcePath)
        {
            ArgumentNullException.ThrowIfNull(gamePath);
            ArgumentNullException.ThrowIfNull(lmmSourcePath);

            var patchFilesPath = Path.Combine(lmmSourcePath, PatchFilesSubdir);

            if (!_fileSystem.DirectoryExists(patchFilesPath))
            {
                throw new DirectoryNotFoundException(
                    $"LMM PatchFiles directory not found: {patchFilesPath}\n" +
                    $"Extract the LobotomyModManager tool into: {lmmSourcePath}");
            }

            var gameManagedPath = Path.Combine(gamePath, "LobotomyCorp_Data", "Managed");
            var gameBaseModsPath = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");

            Console.WriteLine("Installing LMM from tool distribution...");

            // Copy patched Assembly-CSharp.dll
            var patchedDll = Path.Combine(patchFilesPath, PatchedAssemblyName);
            if (!_fileSystem.FileExists(patchedDll))
            {
                throw new FileNotFoundException(
                    $"Patched Assembly-CSharp not found: {patchedDll}\n" +
                    "The LMM tool may need to be run first to generate the patched DLL.");
            }

            Console.WriteLine($"Copying {PatchedAssemblyName} -> Managed/{GameAssemblyName}");
            _fileSystem.CopyFile(patchedDll, Path.Combine(gameManagedPath, GameAssemblyName), true);

            // Copy all other DLLs from PatchFiles (excluding vanilla Assembly-CSharp.dll and Lobotomypatch.dll)
            var patchDlls = _fileSystem.GetFiles(patchFilesPath, "*.dll");
            foreach (var dll in patchDlls)
            {
                var fileName = Path.GetFileName(dll);
                if (IsExcludedFile(fileName))
                {
                    continue;
                }

                Console.WriteLine($"Copying {fileName} -> Managed/");
                _fileSystem.CopyFile(dll, Path.Combine(gameManagedPath, fileName), true);
            }

            // Copy BaseMod/ directory into Managed/BaseMod/
            var baseModSource = Path.Combine(patchFilesPath, "BaseMod");
            if (_fileSystem.DirectoryExists(baseModSource))
            {
                var baseModDest = Path.Combine(gameManagedPath, "BaseMod");
                Console.WriteLine("Copying BaseMod/ -> Managed/BaseMod/");
                _fileSystem.CopyDirectory(baseModSource, baseModDest, true);
            }

            // Create BaseMods/BaseModList_v2.xml if it doesn't exist
            if (!_fileSystem.DirectoryExists(gameBaseModsPath))
            {
                _fileSystem.CreateDirectory(gameBaseModsPath);
            }

            var baseModListPath = Path.Combine(gameBaseModsPath, "BaseModList_v2.xml");
            if (!_fileSystem.FileExists(baseModListPath))
            {
                Console.WriteLine("Creating BaseMods/BaseModList_v2.xml");
                _fileSystem.WriteAllText(baseModListPath, BaseModListXml);
            }

            Console.WriteLine("LMM installation complete.");
        }

        private static bool IsExcludedFile(string fileName)
        {
            foreach (var excluded in s_excludedFiles)
            {
                if (string.Equals(fileName, excluded, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
