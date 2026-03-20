// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Interfaces
{
    public sealed class AssemblyInspectionInfo
    {
        public AssemblyInspectionInfo(string name, string version, string location, IList<AssemblyName> references)
        {
            ThrowHelper.ThrowIfNull(name);
            Name = name;
            ThrowHelper.ThrowIfNull(version);
            Version = version;
            Location = location ?? string.Empty;
            ThrowHelper.ThrowIfNull(references);
            References = references;
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public IList<AssemblyName> References { get; private set; }
    }
}
