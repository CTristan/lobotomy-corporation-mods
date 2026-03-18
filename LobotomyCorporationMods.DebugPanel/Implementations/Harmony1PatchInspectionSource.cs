// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.Common.Enums.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    /// <summary>
    ///     Reflects into Harmony 1.09's internal HarmonySharedState to extract active patch data.
    ///     Scans ALL assemblies containing HarmonySharedState (not just the first) to handle
    ///     BepInEx environments where multiple Harmony assemblies coexist.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class Harmony1PatchInspectionSource : IPatchInspectionSource
    {
        private const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly List<string> s_lastDiagnostics = new List<string>();

        public static IList<string> LastDiagnostics => s_lastDiagnostics;

        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            var patches = new List<PatchInspectionInfo>();
            var seenTargets = new HashSet<string>(StringComparer.Ordinal);

            s_lastDiagnostics.Clear();

            try
            {
                var sharedStateTypes = new List<TypeAssemblyPair>();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    if (asm == null)
                    {
                        continue;
                    }

                    var sharedStateType = asm.GetType("Harmony.HarmonySharedState");
                    if (sharedStateType != null)
                    {
                        sharedStateTypes.Add(new TypeAssemblyPair(sharedStateType, asm));
                    }
                }

                s_lastDiagnostics.Add("Harmony1: found " + sharedStateTypes.Count + " assemblies with Harmony.HarmonySharedState");

                if (sharedStateTypes.Count == 0)
                {
                    return patches;
                }

                foreach (var pair in sharedStateTypes)
                {
                    try
                    {
                        var assemblyName = pair.Assembly.GetName().Name ?? "Unknown";
                        var patchesFromState = ExtractPatchesFromSharedState(pair.SharedStateType, seenTargets);

                        s_lastDiagnostics.Add("Harmony1: " + assemblyName + " (v" +
                            (pair.Assembly.GetName().Version != null ? pair.Assembly.GetName().Version.ToString() : "?") +
                            "): " + patchesFromState.Count + " patches");

                        patches.AddRange(patchesFromState);
                    }
                    catch (Exception ex)
                    {
                        s_lastDiagnostics.Add("Harmony1: error querying assembly: " + ex.GetType().Name + ": " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                s_lastDiagnostics.Add("Harmony1: top-level error: " + ex.GetType().Name + ": " + ex.Message);
            }

            s_lastDiagnostics.Add("Harmony1: total patches from all shared states: " + patches.Count);

            return patches;
        }

        private static List<PatchInspectionInfo> ExtractPatchesFromSharedState(Type sharedStateType, HashSet<string> seenTargets)
        {
            var patches = new List<PatchInspectionInfo>();

            var getStateMethod = sharedStateType.GetMethod("GetState", NonPublicStatic);
            if (getStateMethod == null)
            {
                return patches;
            }

            if (!(getStateMethod.Invoke(null, null) is IDictionary state))
            {
                return patches;
            }

            var getPatchInfoMethod = sharedStateType.GetMethod("GetPatchInfo", NonPublicStatic);

            foreach (DictionaryEntry entry in state)
            {
                if (!(entry.Key is MethodBase targetMethod))
                {
                    continue;
                }

                var targetKey = (targetMethod.DeclaringType != null ? targetMethod.DeclaringType.FullName : "") + "." + targetMethod.Name;
                if (!seenTargets.Add(targetKey))
                {
                    continue;
                }

                try
                {
                    if (getPatchInfoMethod != null)
                    {
                        var patchInfo = getPatchInfoMethod.Invoke(null, new object[] { targetMethod });
                        if (patchInfo != null)
                        {
                            AddPatchesFromPatchInfo(patches, patchInfo, targetMethod);
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip individual patch extraction failures
                }
            }

            return patches;
        }

        private sealed class TypeAssemblyPair
        {
            public TypeAssemblyPair(Type sharedStateType, Assembly assembly)
            {
                SharedStateType = sharedStateType;
                Assembly = assembly;
            }

            public Type SharedStateType { get; }

            public Assembly Assembly { get; }
        }

        private static void AddPatchesFromPatchInfo(List<PatchInspectionInfo> destination, object patchInfo, MethodBase targetMethod)
        {
            var patchInfoType = patchInfo.GetType();

            AddPatchesFromArray(destination, patchInfoType, patchInfo, "prefixes", targetMethod, PatchType.Prefix);
            AddPatchesFromArray(destination, patchInfoType, patchInfo, "postfixes", targetMethod, PatchType.Postfix);
            AddPatchesFromArray(destination, patchInfoType, patchInfo, "transpilers", targetMethod, PatchType.Transpiler);
        }

        private static void AddPatchesFromArray(
            List<PatchInspectionInfo> destination,
            Type patchInfoType,
            object patchInfo,
            string fieldName,
            MethodBase targetMethod,
            PatchType patchType)
        {
            var field = patchInfoType.GetField(fieldName, InstanceFields);
            if (field == null)
            {
                return;
            }

            if (!(field.GetValue(patchInfo) is Array patchArray))
            {
                return;
            }

            foreach (var patch in patchArray)
            {
                if (patch == null)
                {
                    continue;
                }

                try
                {
                    var patchObjType = patch.GetType();

                    var ownerField = patchObjType.GetField("owner", InstanceFields);
                    var owner = ownerField != null ? ownerField.GetValue(patch) as string ?? string.Empty : string.Empty;

                    var patchMethodField = patchObjType.GetField("patch", InstanceFields);
                    var patchMethodInfo = patchMethodField != null ? patchMethodField.GetValue(patch) as MethodInfo : null;

                    if (patchMethodInfo == null)
                    {
                        continue;
                    }

                    ExtractPatchAssemblyInfo(patchMethodInfo, out var assemblyName, out var assemblyVersion, out var assemblyReferences);

                    var targetTypeName = targetMethod.DeclaringType != null
                        ? targetMethod.DeclaringType.FullName ?? targetMethod.DeclaringType.Name
                        : "Unknown";

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
