// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class DetectedModInfo
    {
        public DetectedModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier, bool hasActivePatches, int activePatchCount, int expectedPatchCount)
        {
            ThrowHelper.ThrowIfNull(name);
            Name = name;
            ThrowHelper.ThrowIfNull(version);
            Version = version;
            Source = source;
            HarmonyVersion = harmonyVersion;
            ThrowHelper.ThrowIfNull(assemblyName);
            AssemblyName = assemblyName;
            ThrowHelper.ThrowIfNull(identifier);
            Identifier = identifier;
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

        public void SetPatchInfo(bool hasActivePatches, int activePatchCount)
        {
            HasActivePatches = hasActivePatches;
            ActivePatchCount = activePatchCount;
        }

        public void SetExpectedPatchCount(int expectedPatchCount)
        {
            ExpectedPatchCount = expectedPatchCount;
        }
    }
}
