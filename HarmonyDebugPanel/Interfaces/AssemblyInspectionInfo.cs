// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class AssemblyInspectionInfo
    {
        public AssemblyInspectionInfo(string name, string version, string location, IList<AssemblyName> references)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Location = location ?? string.Empty;
            References = references ?? throw new ArgumentNullException(nameof(references));
        }

        public string Name { get; private set; }

        public string Version { get; private set; }

        public string Location { get; private set; }

        public IList<AssemblyName> References { get; private set; }
    }
}
