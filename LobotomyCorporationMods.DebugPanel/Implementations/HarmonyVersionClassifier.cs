// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.Common.Enums.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class HarmonyVersionClassifier : IHarmonyVersionClassifier
    {
        private const string Harmony1AssemblyName = "0Harmony109";
        private const string Harmony2AssemblyName = "0Harmony";
        private const string Harmony12AssemblyName = "12Harmony";
        private const string Harmony012AssemblyName = "0Harmony12";

        public HarmonyVersion Classify(IList<AssemblyName> references)
        {
            if (references == null || references.Count == 0)
            {
                return HarmonyVersion.Unknown;
            }

            var hasHarmony1 = false;
            var hasHarmony2 = false;

            foreach (var reference in references)
            {
                if (reference == null)
                {
                    continue;
                }

                var referenceName = reference.Name ?? string.Empty;
                if (referenceName.Equals(Harmony1AssemblyName, StringComparison.OrdinalIgnoreCase) ||
                    referenceName.Equals(Harmony12AssemblyName, StringComparison.OrdinalIgnoreCase) ||
                    referenceName.Equals(Harmony012AssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    hasHarmony1 = true;
                }
                else if (referenceName.Equals(Harmony2AssemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    hasHarmony2 = true;
                }
            }

            return hasHarmony1 ? HarmonyVersion.Harmony1 : hasHarmony2 ? HarmonyVersion.Harmony2 : HarmonyVersion.Unknown;
        }
    }
}
