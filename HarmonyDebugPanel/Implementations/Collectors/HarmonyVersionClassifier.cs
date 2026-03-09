// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
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

            bool hasHarmony1 = false;
            bool hasHarmony2 = false;

            foreach (AssemblyName reference in references)
            {
                if (reference == null)
                {
                    continue;
                }

                string referenceName = reference.Name ?? string.Empty;
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
