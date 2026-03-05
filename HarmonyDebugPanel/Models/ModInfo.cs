// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class ModInfo
    {
        public ModInfo()
            : this(string.Empty, string.Empty, ModSource.Lmm, HarmonyVersion.Unknown, string.Empty, string.Empty)
        {
        }

        public ModInfo(string name, string version, ModSource source, HarmonyVersion harmonyVersion, string assemblyName, string identifier)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Source = source;
            HarmonyVersion = harmonyVersion;
            AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public ModSource Source { get; private set; }

        public HarmonyVersion HarmonyVersion { get; private set; }

        public string AssemblyName { get; private set; }

        public string Identifier { get; private set; }
    }
}
