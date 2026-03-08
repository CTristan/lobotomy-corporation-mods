// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class ModInfo
    {
        public ModInfo()
            : this(string.Empty, string.Empty, ModSource.Lmm, HarmonyVersion.Unknown, string.Empty, string.Empty, false, 0, 0)
        {
        }

        public ModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier)
            : this(name, version, source, harmonyVersion, assemblyName, identifier, false, 0, 0)
        {
        }

        public ModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier, bool hasActivePatches, int activePatchCount)
            : this(name, version, source, harmonyVersion, assemblyName, identifier, hasActivePatches, activePatchCount, 0)
        {
        }

        public ModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier, bool hasActivePatches, int activePatchCount, int expectedPatchCount)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Source = source;
            HarmonyVersion = harmonyVersion;
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            HasActivePatches = hasActivePatches;
            ActivePatchCount = activePatchCount;
            ExpectedPatchCount = expectedPatchCount;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public ModSource Source { get; private set; }

        public HarmonyVersion HarmonyVersion { get; private set; }

        public string AssemblyName { get; private set; }

        public string Identifier { get; private set; }

        public bool HasActivePatches { get; private set; }

        public int ActivePatchCount { get; private set; }

        public int ExpectedPatchCount { get; private set; }

        internal void SetPatchInfo(bool hasActivePatches, int activePatchCount)
        {
            HasActivePatches = hasActivePatches;
            ActivePatchCount = activePatchCount;
        }

        internal void SetExpectedPatchCount(int expectedPatchCount)
        {
            ExpectedPatchCount = expectedPatchCount;
        }
    }
}
