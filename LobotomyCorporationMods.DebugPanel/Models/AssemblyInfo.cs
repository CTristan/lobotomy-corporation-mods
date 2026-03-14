// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class AssemblyInfo
    {
        public AssemblyInfo(string name, string version, string location, bool isHarmonyRelated)
        {
            Name = Guard.Against.Null(name, nameof(name));
            Version = Guard.Against.Null(version, nameof(version));
            Location = Guard.Against.Null(location, nameof(location));
            IsHarmonyRelated = isHarmonyRelated;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public bool IsHarmonyRelated { get; private set; }
    }
}
