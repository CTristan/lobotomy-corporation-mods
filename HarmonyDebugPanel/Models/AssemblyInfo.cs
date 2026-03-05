// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class AssemblyInfo
    {
        public AssemblyInfo()
            : this(string.Empty, string.Empty, string.Empty, isHarmonyRelated: false)
        {
        }

        public AssemblyInfo(string name, string version, string location, bool isHarmonyRelated)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            IsHarmonyRelated = isHarmonyRelated;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public bool IsHarmonyRelated { get; private set; }
    }
}
