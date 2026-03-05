// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using HarmonyLib;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    [ExcludeFromCodeCoverage(Justification = "Requires live Harmony patch state")]
    public sealed class HarmonyPatchInspectionSource : IPatchInspectionSource
    {
        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            var patches = new List<PatchInspectionInfo>();
            foreach (var method in Harmony.GetAllPatchedMethods())
            {
                if (method == null)
                {
                    continue;
                }

                var patchInfo = Harmony.GetPatchInfo(method);
                if (patchInfo == null)
                {
                    continue;
                }

                AddPatches(patches, patchInfo.Prefixes, method, PatchType.Prefix);
                AddPatches(patches, patchInfo.Postfixes, method, PatchType.Postfix);
                AddPatches(patches, patchInfo.Transpilers, method, PatchType.Transpiler);
                AddPatches(patches, patchInfo.Finalizers, method, PatchType.Finalizer);
            }

            return patches;
        }

        private static void AddPatches(List<PatchInspectionInfo> destination, IEnumerable<Patch> source, MethodBase targetMethod, PatchType patchType)
        {
            if (source == null)
            {
                return;
            }

            foreach (var patch in source)
            {
                if (patch == null)
                {
                    continue;
                }

                destination.Add(new PatchInspectionInfo(
                    targetMethod.DeclaringType != null ? targetMethod.DeclaringType.FullName : "Unknown",
                    targetMethod.Name,
                    patchType,
                    patch.owner ?? string.Empty,
                    GetMethodDisplayName(patch.PatchMethod)));
            }
        }

        private static string GetMethodDisplayName(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return string.Empty;
            }

            var declaringTypeName = methodInfo.DeclaringType != null
                ? methodInfo.DeclaringType.FullName
                : "Unknown";
            return declaringTypeName + "." + methodInfo.Name;
        }
    }
}
