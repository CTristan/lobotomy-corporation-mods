// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Harmony2ForLmm.Interfaces;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Provides access to resources embedded in the assembly.
    /// </summary>
    public sealed class EmbeddedResourceProvider(Assembly assembly) : IResourceProvider
    {
        /// <summary>
        /// Initializes a new instance using the executing assembly.
        /// </summary>
        public EmbeddedResourceProvider()
            : this(Assembly.GetExecutingAssembly())
        {
        }

        /// <inheritdoc />
        public string? ReadDocumentText(string fileName)
        {
            var stream = assembly.GetManifestResourceStream($"Resources.docs.{fileName}");
            if (stream == null)
            {
                return null;
            }

            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <inheritdoc />
        public void ExtractBepInExTo(string targetDirectory, ICollection<string> filesWritten)
        {
            ArgumentNullException.ThrowIfNull(filesWritten);

            var stream = assembly.GetManifestResourceStream("Resources.bepinex.zip");
            if (stream == null)
            {
                return;
            }

            using (stream)
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    var destPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));

                    // Ensure the entry doesn't escape the target directory (zip slip protection)
                    var fullTargetDir = Path.GetFullPath(targetDirectory + Path.DirectorySeparatorChar);
                    if (!destPath.StartsWith(fullTargetDir, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null)
                    {
                        _ = Directory.CreateDirectory(destDir);
                    }

                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                    {
                        entryStream.CopyTo(fileStream);
                    }

                    filesWritten.Add(destPath);
                }
            }
        }

        /// <inheritdoc />
        public void CopyDllTo(string resourceName, string destinationPath, ICollection<string> filesWritten)
        {
            ArgumentNullException.ThrowIfNull(filesWritten);

            var stream = assembly.GetManifestResourceStream($"Resources.{resourceName}");
            if (stream == null)
            {
                return;
            }

            using (stream)
            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            filesWritten.Add(destinationPath);
        }

        /// <inheritdoc />
        public Stream? OpenDemoModZip()
        {
            return assembly.GetManifestResourceStream("Resources.DemoMod.zip");
        }

        /// <inheritdoc />
        public void ExtractDebugPanelTo(string baseModsPath, ICollection<string> filesWritten)
        {
            ArgumentNullException.ThrowIfNull(filesWritten);

            var stream = assembly.GetManifestResourceStream("Resources.debugpanel.zip");
            if (stream == null)
            {
                return;
            }

            var targetDirectory = Path.Combine(baseModsPath, "DebugPanel");

            using (stream)
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        continue;
                    }

                    var destPath = Path.GetFullPath(Path.Combine(targetDirectory, entry.FullName));

                    // Ensure the entry doesn't escape the target directory (zip slip protection)
                    var fullTargetDir = Path.GetFullPath(targetDirectory + Path.DirectorySeparatorChar);
                    if (!destPath.StartsWith(fullTargetDir, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null)
                    {
                        _ = Directory.CreateDirectory(destDir);
                    }

                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                    {
                        entryStream.CopyTo(fileStream);
                    }

                    filesWritten.Add(destPath);
                }
            }
        }
    }
}
