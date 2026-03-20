// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Provides access to embedded resource files for installation and documentation.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Reads a documentation file as text.
        /// </summary>
        /// <param name="fileName">The document file name (e.g., "UsersGuide.md").</param>
        /// <returns>The document content, or null if the resource is not found.</returns>
        string? ReadDocumentText(string fileName);

        /// <summary>
        /// Extracts the BepInEx distribution archive to the target directory.
        /// </summary>
        /// <param name="targetDirectory">The directory to extract files into.</param>
        /// <param name="filesWritten">Collection to record paths of written files.</param>
        void ExtractBepInExTo(string targetDirectory, ICollection<string> filesWritten);

        /// <summary>
        /// Copies an embedded DLL resource to a destination path.
        /// </summary>
        /// <param name="resourceName">The DLL resource name (e.g., "RetargetHarmony.dll").</param>
        /// <param name="destinationPath">The full path to write the DLL to.</param>
        /// <param name="filesWritten">Collection to record paths of written files.</param>
        void CopyDllTo(string resourceName, string destinationPath, ICollection<string> filesWritten);

        /// <summary>
        /// Opens the DemoMod.zip resource as a stream.
        /// </summary>
        /// <returns>A stream for the DemoMod.zip, or null if not available. Caller must dispose.</returns>
        Stream? OpenDemoModZip();

        /// <summary>
        /// Extracts the DebugPanel distribution archive to the target BaseMods path.
        /// </summary>
        /// <param name="baseModsPath">The BaseMods directory to extract DebugPanel into.</param>
        /// <param name="filesWritten">Collection to record paths of written files.</param>
        void ExtractDebugPanelTo(string baseModsPath, ICollection<string> filesWritten);
    }
}
