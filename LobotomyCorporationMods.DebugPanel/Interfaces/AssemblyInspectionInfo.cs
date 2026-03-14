// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public sealed class AssemblyInspectionInfo
    {
        public AssemblyInspectionInfo(string name, string version, string location, IList<AssemblyName> references)
        {
            Name = Guard.Against.Null(name, nameof(name));
            Version = Guard.Against.Null(version, nameof(version));
            Location = location ?? string.Empty;
            References = Guard.Against.Null(references, nameof(references));
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public IList<AssemblyName> References { get; private set; }
    }
}
