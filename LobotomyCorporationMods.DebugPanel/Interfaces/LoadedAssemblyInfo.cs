// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public sealed class LoadedAssemblyInfo
    {
        public LoadedAssemblyInfo(string name, string location, IList<string> referenceNames)
        {
            ThrowHelper.ThrowIfNull(name);
            Name = name;
            ThrowHelper.ThrowIfNull(location);
            Location = location;
            ThrowHelper.ThrowIfNull(referenceNames);
            ReferenceNames = referenceNames;
        }

        public string Name { get; private set; }

        public string Location { get; private set; }

        public IList<string> ReferenceNames { get; private set; }
    }
}
