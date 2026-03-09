// SPDX-License-Identifier: MIT

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
            List<PatchInspectionInfo> patches = [];
            foreach (MethodBase method in Harmony.GetAllPatchedMethods())
            {
                if (method == null)
                {
                    continue;
                }

                Patches patchInfo = Harmony.GetPatchInfo(method);
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

            foreach (Patch patch in source)
            {
                if (patch == null)
                {
                    continue;
                }

                ExtractPatchAssemblyInfo(patch.PatchMethod, out string assemblyName, out string assemblyVersion, out IList<AssemblyName> assemblyReferences);

                destination.Add(new PatchInspectionInfo(
                    targetMethod.DeclaringType != null ? targetMethod.DeclaringType.FullName : "Unknown",
                    targetMethod.Name,
                    patchType,
                    patch.owner ?? string.Empty,
                    GetMethodDisplayName(patch.PatchMethod),
                    assemblyName,
                    assemblyVersion,
                    assemblyReferences));
            }
        }

        private static void ExtractPatchAssemblyInfo(MethodInfo methodInfo, out string assemblyName, out string assemblyVersion, out IList<AssemblyName> assemblyReferences)
        {
            assemblyName = string.Empty;
            assemblyVersion = string.Empty;
            assemblyReferences = [];

            if (methodInfo == null || methodInfo.DeclaringType == null)
            {
                return;
            }

            Assembly assembly = methodInfo.DeclaringType.Assembly;
            if (assembly == null)
            {
                return;
            }

            AssemblyName assemblyObj = assembly.GetName();
            assemblyName = assemblyObj.Name ?? string.Empty;
            assemblyVersion = assemblyObj.Version != null ? assemblyObj.Version.ToString() : string.Empty;

            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            if (referencedAssemblies != null)
            {
                assemblyReferences = [.. referencedAssemblies];
            }
        }

        private static string GetMethodDisplayName(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                return string.Empty;
            }

            string declaringTypeName = methodInfo.DeclaringType != null
                ? methodInfo.DeclaringType.FullName
                : "Unknown";
            return declaringTypeName + "." + methodInfo.Name;
        }
    }
}
