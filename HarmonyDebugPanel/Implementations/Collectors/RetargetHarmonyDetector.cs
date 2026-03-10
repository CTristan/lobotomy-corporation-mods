// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class RetargetHarmonyDetector(IAssemblyInspectionSource assemblySource) : IInfoCollector<RetargetHarmonyStatus>
    {
        private readonly IAssemblyInspectionSource _assemblySource = assemblySource ?? throw new ArgumentNullException(nameof(assemblySource));

        public RetargetHarmonyDetector()
            : this(new AppDomainAssemblyInspectionSource())
        {
        }

        public RetargetHarmonyStatus Collect()
        {
            var assemblies = new List<AssemblyInspectionInfo>(_assemblySource.GetAssemblies());

            var isDetected = false;
            AssemblyInspectionInfo assemblyCSharp = null;
            AssemblyInspectionInfo lobotomyBaseModLib = null;

            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                {
                    continue;
                }

                if (assembly.Name.Equals("RetargetHarmony", StringComparison.OrdinalIgnoreCase))
                {
                    isDetected = true;
                }

                if (assembly.Name.Equals("Assembly-CSharp", StringComparison.OrdinalIgnoreCase))
                {
                    assemblyCSharp = assembly;
                }

                if (assembly.Name.Equals("LobotomyBaseModLib", StringComparison.OrdinalIgnoreCase))
                {
                    lobotomyBaseModLib = assembly;
                }
            }

            var assemblyCSharpRetargeted = HasHarmony109Reference(assemblyCSharp);
            var lobotomyBaseModLibRetargeted = HasHarmony109Reference(lobotomyBaseModLib);
            var message = BuildMessage(isDetected, assemblyCSharpRetargeted, lobotomyBaseModLibRetargeted);

            return new RetargetHarmonyStatus(
                isDetected,
                assemblyCSharpRetargeted,
                lobotomyBaseModLibRetargeted,
                message);
        }

        private static bool HasHarmony109Reference(AssemblyInspectionInfo assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            foreach (var reference in assembly.References)
            {
                if (reference != null && reference.Name.Equals("0Harmony109", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildMessage(bool isDetected, bool assemblyCSharpRetargeted, bool lobotomyBaseModLibRetargeted)
        {
            if (!isDetected)
            {
                return "Not detected";
            }

            var patchedAssemblies = new List<string>();
            if (assemblyCSharpRetargeted)
            {
                patchedAssemblies.Add("Assembly-CSharp");
            }

            if (lobotomyBaseModLibRetargeted)
            {
                patchedAssemblies.Add("LobotomyBaseModLib");
            }

            if (patchedAssemblies.Count == 0)
            {
                return "Detected (target assemblies not patched yet)";
            }

            return "Detected (patched " + string.Join(", ", patchedAssemblies.ToArray()) + ")";
        }
    }
}
