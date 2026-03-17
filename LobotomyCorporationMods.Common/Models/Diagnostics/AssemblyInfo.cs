// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class AssemblyInfo
    {
        public AssemblyInfo(string name, string version, string location, bool isHarmonyRelated, IList<AssemblyName> references)
        {
            ThrowHelper.ThrowIfNull(name);
            Name = name;
            ThrowHelper.ThrowIfNull(version);
            Version = version;
            ThrowHelper.ThrowIfNull(location);
            Location = location;
            IsHarmonyRelated = isHarmonyRelated;
            ThrowHelper.ThrowIfNull(references);
            References = references;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public bool IsHarmonyRelated { get; private set; }

        public IList<AssemblyName> References { get; private set; }
    }
}
