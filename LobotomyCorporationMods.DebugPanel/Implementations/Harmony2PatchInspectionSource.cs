// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Enums.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    /// <summary>
    ///     Reflects into Harmony 2's HarmonyLib.Harmony API to extract all active patch data.
    ///     This is the preferred source when Harmony 2 is available, as it sees both Harmony 1 and 2 patches.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class Harmony2PatchInspectionSource : IPatchInspectionSource
    {
        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        private static readonly List<string> s_lastDiagnostics = new List<string>();

        public static IList<string> LastDiagnostics => s_lastDiagnostics;

        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            var patches = new List<PatchInspectionInfo>();

            s_lastDiagnostics.Clear();

            try
            {
                Type harmonyType = null;
                string sourceAssemblyName = null;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    harmonyType = assembly.GetType("HarmonyLib.Harmony");
                    if (harmonyType != null)
                    {
                        sourceAssemblyName = assembly.GetName().Name ?? "Unknown";

                        break;
                    }
                }

                if (harmonyType == null)
                {
                    s_lastDiagnostics.Add("Harmony2: HarmonyLib.Harmony type not found in any assembly");

                    return patches;
                }

                s_lastDiagnostics.Add("Harmony2: found HarmonyLib.Harmony in " + sourceAssemblyName);

                var getAllPatchedMethods = harmonyType.GetMethod("GetAllPatchedMethods", PublicStatic);
                var getPatchInfo = harmonyType.GetMethod("GetPatchInfo", PublicStatic, null, new[] { typeof(MethodBase) }, null);

                if (getAllPatchedMethods == null || getPatchInfo == null)
                {
                    s_lastDiagnostics.Add("Harmony2: GetAllPatchedMethods=" + (getAllPatchedMethods != null ? "found" : "null") +
                        ", GetPatchInfo=" + (getPatchInfo != null ? "found" : "null"));

                    return patches;
                }

                if (!(getAllPatchedMethods.Invoke(null, null) is IEnumerable methods))
                {
                    s_lastDiagnostics.Add("Harmony2: GetAllPatchedMethods returned null or non-IEnumerable");

                    return patches;
                }

                var methodCount = 0;
                var nullPatchInfoCount = 0;
                var totalPrefixes = 0;
                var totalPostfixes = 0;
                var totalTranspilers = 0;
                var totalFinalizers = 0;
                var nullPatchMethodCount = 0;

                foreach (var methodObj in methods)
                {
                    if (!(methodObj is MethodBase method))
                    {
                        continue;
                    }

                    methodCount++;

                    try
                    {
                        var patchesObj = getPatchInfo.Invoke(null, new object[] { method });
                        if (patchesObj == null)
                        {
                            nullPatchInfoCount++;

                            continue;
                        }

                        var patchesType = patchesObj.GetType();
                        var beforeCount = patches.Count;
                        var beforeNullPatchMethod = nullPatchMethodCount;

                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Prefixes", method, PatchType.Prefix, ref nullPatchMethodCount);
                        totalPrefixes += CountCollectionProperty(patchesType, patchesObj, "Prefixes");
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Postfixes", method, PatchType.Postfix, ref nullPatchMethodCount);
                        totalPostfixes += CountCollectionProperty(patchesType, patchesObj, "Postfixes");
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Transpilers", method, PatchType.Transpiler, ref nullPatchMethodCount);
                        totalTranspilers += CountCollectionProperty(patchesType, patchesObj, "Transpilers");
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Finalizers", method, PatchType.Finalizer, ref nullPatchMethodCount);
                        totalFinalizers += CountCollectionProperty(patchesType, patchesObj, "Finalizers");

                        var patchesExtracted = patches.Count - beforeCount;
                        var nullsFound = nullPatchMethodCount - beforeNullPatchMethod;
                        if (patchesExtracted > 0 || nullsFound > 0)
                        {
                            var targetName = method.DeclaringType != null
                                ? method.DeclaringType.FullName + "." + method.Name
                                : method.Name;
                            s_lastDiagnostics.Add("Harmony2:   " + targetName + ": " + patchesExtracted + " extracted, " + nullsFound + " null PatchMethod");
                        }
                    }
                    catch (Exception)
                    {
                        // Skip individual method inspection failures
                    }
                }

                s_lastDiagnostics.Add("Harmony2: " + methodCount + " patched methods enumerated, " + patches.Count + " patches extracted");
                s_lastDiagnostics.Add("Harmony2: collection counts: Prefixes=" + totalPrefixes + " Postfixes=" + totalPostfixes + " Transpilers=" + totalTranspilers + " Finalizers=" + totalFinalizers);
                s_lastDiagnostics.Add("Harmony2: nullPatchInfo=" + nullPatchInfoCount + " nullPatchMethod=" + nullPatchMethodCount);
            }
            catch (Exception ex)
            {
                s_lastDiagnostics.Add("Harmony2: top-level error: " + ex.GetType().Name + ": " + ex.Message);
            }

            return patches;
        }

        private static int CountCollectionProperty(Type patchesType, object patchesObj, string propertyName)
        {
            try
            {
                var property = patchesType.GetProperty(propertyName, PublicInstance);
                if (property == null)
                {
                    return 0;
                }

                if (!(property.GetValue(patchesObj, null) is IEnumerable collection))
                {
                    return 0;
                }

                var count = 0;
                foreach (var item in collection)
                {
                    if (item != null)
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void AddPatchesFromProperty(
            List<PatchInspectionInfo> destination,
            Type patchesType,
            object patchesObj,
            string propertyName,
            MethodBase targetMethod,
            PatchType patchType,
            ref int nullPatchMethodCount)
        {
            var property = patchesType.GetProperty(propertyName, PublicInstance);
            if (property == null)
            {
                return;
            }

            if (!(property.GetValue(patchesObj, null) is IEnumerable patchCollection))
            {
                return;
            }

            foreach (var patch in patchCollection)
            {
                if (patch == null)
                {
                    continue;
                }

                try
                {
                    var patchObjType = patch.GetType();

                    var ownerProp = patchObjType.GetProperty("owner", PublicInstance)
                                    ?? patchObjType.GetProperty("Owner", PublicInstance);
                    var owner = ownerProp != null ? ownerProp.GetValue(patch, null) as string ?? string.Empty : string.Empty;

                    var patchMethodProp = patchObjType.GetProperty("PatchMethod", PublicInstance);
                    var patchMethodInfo = patchMethodProp != null ? patchMethodProp.GetValue(patch, null) as MethodInfo : null;

                    var targetTypeName = targetMethod.DeclaringType != null
                        ? targetMethod.DeclaringType.FullName ?? targetMethod.DeclaringType.Name
                        : "Unknown";

                    if (patchMethodInfo != null)
                    {
                        ExtractPatchAssemblyInfo(patchMethodInfo, out var assemblyName, out var assemblyVersion, out var assemblyReferences);

                        destination.Add(new PatchInspectionInfo(
                            targetTypeName,
                            targetMethod.Name,
                            patchType,
                            owner,
                            GetMethodDisplayName(patchMethodInfo),
                            assemblyName,
                            assemblyVersion,
                            assemblyReferences));
                    }
                    else
                    {
                        nullPatchMethodCount++;

                        // HarmonyXInterop native detours don't have PatchMethod — derive assembly name from owner
                        var derivedAssemblyName = owner.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                            ? owner.Substring(0, owner.Length - 4)
                            : owner;

                        destination.Add(new PatchInspectionInfo(
                            targetTypeName,
                            targetMethod.Name,
                            patchType,
                            owner,
                            owner,
                            derivedAssemblyName,
                            string.Empty,
                            new List<AssemblyName>()));
                    }
                }
                catch (Exception)
                {
                    // Skip individual patch entries that fail
                }
            }
        }

        private static void ExtractPatchAssemblyInfo(
            MethodInfo methodInfo,
            out string assemblyName,
            out string assemblyVersion,
            out IList<AssemblyName> assemblyReferences)
        {
            assemblyName = string.Empty;
            assemblyVersion = string.Empty;
            assemblyReferences = new List<AssemblyName>();

            if (methodInfo == null || methodInfo.DeclaringType == null)
            {
                return;
            }

            var assembly = methodInfo.DeclaringType.Assembly;
            if (assembly == null)
            {
                return;
            }

            var assemblyObj = assembly.GetName();
            assemblyName = assemblyObj.Name ?? string.Empty;
            assemblyVersion = assemblyObj.Version != null ? assemblyObj.Version.ToString() : string.Empty;

            var referencedAssemblies = assembly.GetReferencedAssemblies();
            if (referencedAssemblies != null)
            {
                assemblyReferences = new List<AssemblyName>(referencedAssemblies);
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
