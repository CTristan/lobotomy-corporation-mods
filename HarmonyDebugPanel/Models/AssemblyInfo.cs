// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class AssemblyInfo(string name, string version, string location, bool isHarmonyRelated)
    {
        public AssemblyInfo()
            : this(string.Empty, string.Empty, string.Empty, isHarmonyRelated: false)
        {
        }

        public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

        public string Version { get; private set; } = version ?? throw new ArgumentNullException(nameof(version));

        public string Location { get; private set; } = location ?? throw new ArgumentNullException(nameof(location));

        public bool IsHarmonyRelated { get; private set; } = isHarmonyRelated;
    }
}
