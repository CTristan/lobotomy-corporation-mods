// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace Harmony2ForLmm.Interfaces
{
    /// <summary>
    /// Analyzes DLLs in the BaseMods directory for BepInEx or Harmony 2+ dependencies.
    /// </summary>
    public interface IBaseModsAnalyzer
    {
        /// <summary>
        /// Scans the BaseMods directory for DLLs that reference BepInEx or Harmony 2+.
        /// </summary>
        /// <param name="gamePath">The game installation directory.</param>
        /// <returns>A list of flagged mods with their dependency information.</returns>
        IReadOnlyList<FlaggedMod> Analyze(string gamePath);
    }

    /// <summary>
    /// A mod that was flagged as having a BepInEx or Harmony 2+ dependency.
    /// </summary>
    /// <param name="filePath">The file path of the flagged DLL.</param>
    /// <param name="fileName">The file name of the flagged DLL.</param>
    /// <param name="reason">The type of dependency that triggered the flag.</param>
    /// <param name="referencedAssembly">The name of the referenced assembly that triggered the flag.</param>
    public sealed class FlaggedMod(string filePath, string fileName, FlagReason reason, string referencedAssembly)
    {
        /// <summary>
        /// Gets the file path of the flagged DLL.
        /// </summary>
        public string FilePath { get; } = filePath;

        /// <summary>
        /// Gets the file name of the flagged DLL.
        /// </summary>
        public string FileName { get; } = fileName;

        /// <summary>
        /// Gets the type of dependency that triggered the flag.
        /// </summary>
        public FlagReason Reason { get; } = reason;

        /// <summary>
        /// Gets the name of the referenced assembly that triggered the flag.
        /// </summary>
        public string ReferencedAssembly { get; } = referencedAssembly;
    }

    /// <summary>
    /// The reason a mod was flagged during analysis.
    /// </summary>
    public enum FlagReason
    {
        /// <summary>
        /// The mod references Harmony version 2.0 or higher.
        /// </summary>
        Harmony2Reference,

        /// <summary>
        /// The mod references a BepInEx assembly.
        /// </summary>
        BepInExReference,
    }
}
