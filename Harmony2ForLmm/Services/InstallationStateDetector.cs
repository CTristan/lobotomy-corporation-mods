// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Models;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Detects the current installation state by reading the manifest and verifying files.
    /// </summary>
    public sealed class InstallationStateDetector(IManifestService manifestService, string bundleVersion) : IInstallationStateDetector
    {
        /// <inheritdoc />
        public InstallationStateResult Detect(string gamePath)
        {
            var manifest = manifestService.ReadManifest(gamePath);
            if (manifest is null)
            {
                return InstallationStateResult.Fresh(bundleVersion);
            }

            var missingFiles = FindMissingFiles(gamePath, manifest.Files);
            if (missingFiles.Count > 0)
            {
                return InstallationStateResult.Corrupted(manifest.Version, bundleVersion, missingFiles);
            }

            var comparison = CompareVersions(manifest.Version, bundleVersion);

            var state = comparison switch
            {
                < 0 => InstallationState.Outdated,
                0 => InstallationState.Current,
                _ => InstallationState.Newer,
            };

            return InstallationStateResult.WithState(state, manifest.Version, bundleVersion);
        }

        private static List<string> FindMissingFiles(string gamePath, IEnumerable<string> relativeFiles)
        {
            var missing = new List<string>();
            foreach (var relativePath in relativeFiles)
            {
                var fullPath = Path.Combine(gamePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath))
                {
                    missing.Add(relativePath);
                }
            }

            return missing;
        }

        private static int CompareVersions(string installedVersion, string installerVersion)
        {
            if (Version.TryParse(installedVersion, out var installed) &&
                Version.TryParse(installerVersion, out var installer))
            {
                return installed.CompareTo(installer);
            }

            return StringComparer.Ordinal.Compare(installedVersion, installerVersion);
        }
    }
}
