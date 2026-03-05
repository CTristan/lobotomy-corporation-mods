// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace HarmonyDebugPanel.Models
{
    public sealed class DiagnosticReport
    {
        public DiagnosticReport()
        {
            Mods = new List<ModInfo>();
            Patches = new List<PatchInfo>();
            Assemblies = new List<AssemblyInfo>();
            Warnings = new List<string>();
            RetargetHarmonyStatus = new RetargetHarmonyStatus();
            CollectedAt = DateTime.UtcNow;
        }

        public IList<ModInfo> Mods { get; private set; }

        public IList<PatchInfo> Patches { get; private set; }

        public IList<AssemblyInfo> Assemblies { get; private set; }

        public RetargetHarmonyStatus RetargetHarmonyStatus { get; set; }

        public IList<string> Warnings { get; private set; }

        public DateTime CollectedAt { get; set; }
    }
}
