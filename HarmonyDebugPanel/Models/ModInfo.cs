// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class ModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier, bool hasActivePatches, int activePatchCount, int expectedPatchCount)
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

        public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

        public string Version { get; private set; } = version ?? throw new ArgumentNullException(nameof(version));

        public ModSource Source { get; private set; } = source;

        public HarmonyVersion HarmonyVersion { get; private set; } = harmonyVersion;

        public string AssemblyName { get; private set; } = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

        public string Identifier { get; private set; } = identifier ?? throw new ArgumentNullException(nameof(identifier));

        public bool HasActivePatches { get; private set; } = hasActivePatches;

        public int ActivePatchCount { get; private set; } = activePatchCount;

        public int ExpectedPatchCount { get; private set; } = expectedPatchCount;

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
