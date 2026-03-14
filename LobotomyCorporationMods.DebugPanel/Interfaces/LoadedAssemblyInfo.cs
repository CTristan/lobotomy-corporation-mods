// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public sealed class LoadedAssemblyInfo
    {
        public LoadedAssemblyInfo(string name, string location, IList<string> referenceNames)
        {
            Name = Guard.Against.Null(name, nameof(name));
            Location = Guard.Against.Null(location, nameof(location));
            ReferenceNames = Guard.Against.Null(referenceNames, nameof(referenceNames));
        }

        public string Name { get; private set; }

        public string Location { get; private set; }

        public IList<string> ReferenceNames { get; private set; }
    }
}
