// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class DetectedModInfo
    {
        public DetectedModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier, bool hasActivePatches, int activePatchCount, int expectedPatchCount)
        {
            Name = Guard.Against.Null(name, nameof(name));
            Version = Guard.Against.Null(version, nameof(version));
            Source = source;
            HarmonyVersion = harmonyVersion;
            AssemblyName = Guard.Against.Null(assemblyName, nameof(assemblyName));
            Identifier = Guard.Against.Null(identifier, nameof(identifier));
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
