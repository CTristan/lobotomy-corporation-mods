// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using RetargetHarmony.Installer.Interfaces;

namespace RetargetHarmony.Installer.Services
{
    /// <summary>
    /// Uninstalls BepInEx 5 and RetargetHarmony from a Lobotomy Corporation game directory.
    /// </summary>
    public sealed class UninstallerService(IBaseModsAnalyzer baseModsAnalyzer) : IUninstallerService
    {
        private const string BepInExFolder = "BepInEx";
        private const string PatchersFolder = "patchers";
        private const string CoreFolder = "core";
        private const string RetargetHarmonyFolder = "RetargetHarmony";

        /// <inheritdoc />
        public UninstallResult Uninstall(string gamePath, bool removeBaseMods)
        {
            try
            {
                var filesRemoved = new List<string>();
                var directoriesRemoved = new List<string>();

                if (removeBaseMods)
                {
                    RemoveFlaggedBaseMods(gamePath, filesRemoved);
                }

                RemoveRetargetHarmony(gamePath, filesRemoved, directoriesRemoved);
                RemoveHarmonyInteropDlls(gamePath, filesRemoved);
                RemoveBepInExRootFiles(gamePath, filesRemoved);
                RemoveBepInExDirectory(gamePath, directoriesRemoved);

                return UninstallResult.Success(filesRemoved, directoriesRemoved);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return UninstallResult.Failure($"Uninstallation failed: {ex.Message}");
            }
        }

        private void RemoveFlaggedBaseMods(string gamePath, List<string> filesRemoved)
        {
            var flaggedMods = baseModsAnalyzer.Analyze(gamePath);
            foreach (var mod in flaggedMods)
            {
                if (File.Exists(mod.FilePath))
                {
                    File.Delete(mod.FilePath);
                    filesRemoved.Add(mod.FilePath);
                }
            }
        }

        private static void RemoveRetargetHarmony(string gamePath, List<string> filesRemoved, List<string> directoriesRemoved)
        {
            var patcherDir = Path.Combine(gamePath, BepInExFolder, PatchersFolder, RetargetHarmonyFolder);
            if (Directory.Exists(patcherDir))
            {
                foreach (var file in Directory.GetFiles(patcherDir, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                    filesRemoved.Add(file);
                }

                Directory.Delete(patcherDir, recursive: true);
                directoriesRemoved.Add(patcherDir);
            }
        }

        private static void RemoveHarmonyInteropDlls(string gamePath, List<string> filesRemoved)
        {
            var coreDir = Path.Combine(gamePath, BepInExFolder, CoreFolder);
            string[] harmonyDlls = ["0Harmony109.dll", "0Harmony12.dll"];

            foreach (var dll in harmonyDlls)
            {
                var dllPath = Path.Combine(coreDir, dll);
                if (File.Exists(dllPath))
                {
                    File.Delete(dllPath);
                    filesRemoved.Add(dllPath);
                }
            }
        }

        private static void RemoveBepInExRootFiles(string gamePath, List<string> filesRemoved)
        {
            string[] rootFiles = ["winhttp.dll", "doorstop_config.ini", ".doorstop_version"];

            foreach (var fileName in rootFiles)
            {
                var filePath = Path.Combine(gamePath, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    filesRemoved.Add(filePath);
                }
            }
        }

        private static void RemoveBepInExDirectory(string gamePath, List<string> directoriesRemoved)
        {
            var bepInExDir = Path.Combine(gamePath, BepInExFolder);
            if (Directory.Exists(bepInExDir))
            {
                Directory.Delete(bepInExDir, recursive: true);
                directoriesRemoved.Add(bepInExDir);
            }
        }
    }
}
