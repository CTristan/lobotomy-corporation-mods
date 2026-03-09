// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class AssemblyInspectionInfo(string name, string version, string location, IList<AssemblyName> references)
    {
        public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

        public string Version { get; private set; } = version ?? throw new ArgumentNullException(nameof(version));

        public string Location { get; private set; } = location ?? string.Empty;

        public IList<AssemblyName> References { get; private set; } = references ?? throw new ArgumentNullException(nameof(references));
    }
}
