// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using RetargetHarmony.Installer.Interfaces;

namespace RetargetHarmony.Installer.Services
{
    /// <summary>
    /// Installs BepInEx 5 and RetargetHarmony into a Lobotomy Corporation game directory.
    /// </summary>
    public sealed class InstallerService(string resourcesPath) : IInstallerService
    {
        private const string BepInExFolder = "BepInEx";
        private const string PatchersFolder = "patchers";
        private const string CoreFolder = "core";
        private const string RetargetHarmonyFolder = "RetargetHarmony";

        /// <inheritdoc />
        public bool IsBepInExInstalled(string gamePath)
        {
            return Directory.Exists(Path.Combine(gamePath, BepInExFolder))
                && File.Exists(Path.Combine(gamePath, "winhttp.dll"));
        }

        /// <inheritdoc />
        public bool IsRetargetHarmonyInstalled(string gamePath)
        {
            return File.Exists(Path.Combine(gamePath, BepInExFolder, PatchersFolder, RetargetHarmonyFolder, "RetargetHarmony.dll"));
        }

        /// <inheritdoc />
        public InstallResult Install(string gamePath)
        {
            try
            {
                var filesWritten = new List<string>();

                InstallBepInEx(gamePath, filesWritten);
                InstallRetargetHarmony(gamePath, filesWritten);
                InstallHarmonyInteropDlls(gamePath, filesWritten);

                return InstallResult.Success(filesWritten);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return InstallResult.Failure($"Installation failed: {ex.Message}");
            }
        }

        private void InstallBepInEx(string gamePath, List<string> filesWritten)
        {
            var bepInExResourcePath = Path.Combine(resourcesPath, "bepinex");
            if (!Directory.Exists(bepInExResourcePath))
            {
                return;
            }

            CopyDirectoryRecursive(bepInExResourcePath, gamePath, filesWritten);
        }

        private void InstallRetargetHarmony(string gamePath, List<string> filesWritten)
        {
            var patcherDir = Path.Combine(gamePath, BepInExFolder, PatchersFolder, RetargetHarmonyFolder);
            _ = Directory.CreateDirectory(patcherDir);

            var sourceDll = Path.Combine(resourcesPath, "RetargetHarmony.dll");
            if (File.Exists(sourceDll))
            {
                var destDll = Path.Combine(patcherDir, "RetargetHarmony.dll");
                File.Copy(sourceDll, destDll, overwrite: true);
                filesWritten.Add(destDll);
            }
        }

        private void InstallHarmonyInteropDlls(string gamePath, List<string> filesWritten)
        {
            var coreDir = Path.Combine(gamePath, BepInExFolder, CoreFolder);
            _ = Directory.CreateDirectory(coreDir);

            string[] harmonyDlls = ["0Harmony109.dll", "0Harmony12.dll", "12Harmony.dll"];
            foreach (var dll in harmonyDlls)
            {
                // 12Harmony.dll is copied from 0Harmony12.dll (same library, different assembly name)
                var sourceName = dll == "12Harmony.dll" ? "0Harmony12.dll" : dll;
                var sourceDll = Path.Combine(resourcesPath, sourceName);
                if (File.Exists(sourceDll))
                {
                    var destDll = Path.Combine(coreDir, dll);
                    File.Copy(sourceDll, destDll, overwrite: true);
                    filesWritten.Add(destDll);
                }
            }
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destDir, List<string> filesWritten)
        {
            _ = Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
                filesWritten.Add(destFile);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectoryRecursive(subDir, destSubDir, filesWritten);
            }
        }
    }
}
