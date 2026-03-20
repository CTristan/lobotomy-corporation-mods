// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using Harmony2ForLmm.Interfaces;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Uninstalls BepInEx 5 and RetargetHarmony from a Lobotomy Corporation game directory.
    /// </summary>
    public sealed class UninstallerService(IBaseModsAnalyzer baseModsAnalyzer, IManifestService manifestService) : IUninstallerService
    {
        private const string BepInExFolder = "BepInEx";
        private const string PatchersFolder = "patchers";
        private const string CoreFolder = "core";
        private const string RetargetHarmonyFolder = "RetargetHarmony";

        /// <summary>
        /// Known DebugPanel directory names under BaseMods.
        /// </summary>
        internal static readonly string[] DebugPanelDirectoryNames =
        [
            "DebugPanel",
            "LobotomyCorporationMods.DebugPanel",
            "Hemocode.DebugPanel",
            "HarmonyDebugPanel",
            "LobotomyCorporationMods.HarmonyDebugPanel",
            "Hemocode.HarmonyDebugPanel",
        ];

        /// <inheritdoc />
        public UninstallResult Uninstall(string gamePath, bool removeBaseMods, bool removeDebugPanel = false)
        {
            try
            {
                var filesRemoved = new List<string>();
                var directoriesRemoved = new List<string>();

                if (removeBaseMods)
                {
                    RemoveFlaggedBaseMods(gamePath, filesRemoved);
                }

                if (removeDebugPanel)
                {
                    RemoveDebugPanel(gamePath, directoriesRemoved);
                }

                RestoreShimmedBaseMods(gamePath, filesRemoved);
                RemoveRetargetHarmony(gamePath, filesRemoved, directoriesRemoved);
                RemoveHarmonyInteropDlls(gamePath, filesRemoved);
                RemoveBepInExRootFiles(gamePath, filesRemoved);
                RemoveBepInExDirectory(gamePath, directoriesRemoved);

                manifestService.DeleteManifest(gamePath);

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

        private static void RestoreShimmedBaseMods(string gamePath, List<string> filesRestored)
        {
            var backupDir = Path.Combine(gamePath, "BepInEx_Shim_Backup");
            if (!Directory.Exists(backupDir))
            {
                return;
            }

            var baseModsDir = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");
            foreach (var backupFile in Directory.GetFiles(backupDir, "*.dll", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(backupFile);
                var destinationPath = Path.Combine(baseModsDir, fileName);
                File.Copy(backupFile, destinationPath, overwrite: true);
                filesRestored.Add(destinationPath);
            }

            Directory.Delete(backupDir, recursive: true);
        }

        private static void RemoveDebugPanel(string gamePath, List<string> directoriesRemoved)
        {
            var baseModsDir = Path.Combine(gamePath, "LobotomyCorp_Data", "BaseMods");
            foreach (var dirName in DebugPanelDirectoryNames)
            {
                var dirPath = Path.Combine(baseModsDir, dirName);
                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, recursive: true);
                    directoriesRemoved.Add(dirPath);
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
            string[] harmonyDlls = ["0Harmony109.dll", "0Harmony12.dll", "12Harmony.dll"];

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
