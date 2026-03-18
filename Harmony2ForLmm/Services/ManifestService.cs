// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Harmony2ForLmm.Interfaces;
using Harmony2ForLmm.Models;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Reads and writes the installation manifest at .harmony2forlmm/manifest.json.
    /// </summary>
    public sealed class ManifestService(string bundleVersion) : IManifestService
    {
        private static readonly JsonSerializerOptions s_jsonOptions = new()
        {
            WriteIndented = true,
        };

        /// <inheritdoc />
        public InstallationManifest? ReadManifest(string gamePath)
        {
            var manifestPath = GetManifestPath(gamePath);
            if (!File.Exists(manifestPath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(manifestPath);
                return JsonSerializer.Deserialize<InstallationManifest>(json);
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public void WriteManifest(string gamePath, IReadOnlyList<string> installedFiles)
        {
            ArgumentNullException.ThrowIfNull(installedFiles);

            var manifestDir = Path.Combine(gamePath, IManifestService.ManifestDirectory);
            _ = Directory.CreateDirectory(manifestDir);

            var relativePaths = new System.Collections.ObjectModel.Collection<string>();
            foreach (var file in installedFiles)
            {
                var relativePath = Path.GetRelativePath(gamePath, file)
                    .Replace('\\', '/');
                relativePaths.Add(relativePath);
            }

            var manifest = new InstallationManifest
            {
                Version = bundleVersion,
                InstalledAt = DateTime.UtcNow,
                Files = relativePaths,
            };

            var json = JsonSerializer.Serialize(manifest, s_jsonOptions);
            File.WriteAllText(GetManifestPath(gamePath), json);
        }

        /// <inheritdoc />
        public void DeleteManifest(string gamePath)
        {
            var manifestDir = Path.Combine(gamePath, IManifestService.ManifestDirectory);
            if (Directory.Exists(manifestDir))
            {
                Directory.Delete(manifestDir, recursive: true);
            }
        }

        private static string GetManifestPath(string gamePath)
        {
            return Path.Combine(gamePath, IManifestService.ManifestDirectory, IManifestService.ManifestFileName);
        }
    }
}
