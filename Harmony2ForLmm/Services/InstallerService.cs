// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using Harmony2ForLmm.Interfaces;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Installs BepInEx 5 and RetargetHarmony into a Lobotomy Corporation game directory.
    /// </summary>
    public sealed class InstallerService(IResourceProvider resourceProvider, IManifestService manifestService) : IInstallerService
    {
        private const string BepInExFolder = "BepInEx";
        private const string PatchersFolder = "patchers";
        private const string CoreFolder = "core";
        private const string RetargetHarmonyFolder = "RetargetHarmony";

        /// <inheritdoc />
        public InstallResult Install(string gamePath)
        {
            try
            {
                var filesWritten = new List<string>();

                InstallBepInEx(gamePath, filesWritten);
                InstallRetargetHarmony(gamePath, filesWritten);
                InstallHarmonyInteropDlls(gamePath, filesWritten);
                InstallDocumentation(gamePath, filesWritten);

                manifestService.WriteManifest(gamePath, filesWritten);

                return InstallResult.Success(filesWritten);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return InstallResult.Failure($"Installation failed: {ex.Message}");
            }
        }

        private void InstallBepInEx(string gamePath, List<string> filesWritten)
        {
            resourceProvider.ExtractBepInExTo(gamePath, filesWritten);
        }

        private void InstallRetargetHarmony(string gamePath, List<string> filesWritten)
        {
            var patcherDir = Path.Combine(gamePath, BepInExFolder, PatchersFolder, RetargetHarmonyFolder);
            _ = Directory.CreateDirectory(patcherDir);

            var destDll = Path.Combine(patcherDir, "RetargetHarmony.dll");
            resourceProvider.CopyDllTo("RetargetHarmony.dll", destDll, filesWritten);
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
                var destDll = Path.Combine(coreDir, dll);
                resourceProvider.CopyDllTo(sourceName, destDll, filesWritten);
            }
        }

        private void InstallDocumentation(string gamePath, List<string> filesWritten)
        {
            string[] docFiles = ["UsersGuide.md", "ModdersGuide.md"];
            var docsDir = Path.Combine(gamePath, IManifestService.ManifestDirectory, "docs");

            var anyDocExists = false;
            foreach (var fileName in docFiles)
            {
                var content = resourceProvider.ReadDocumentText(fileName);
                if (content != null)
                {
                    if (!anyDocExists)
                    {
                        _ = Directory.CreateDirectory(docsDir);
                        anyDocExists = true;
                    }

                    var destFile = Path.Combine(docsDir, fileName);
                    File.WriteAllText(destFile, content);
                    filesWritten.Add(destFile);
                }
            }
        }
    }
}
