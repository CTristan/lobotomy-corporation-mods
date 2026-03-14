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
using LobotomyCorporationMods.DebugPanel.Models;

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

        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            var patches = new List<PatchInspectionInfo>();

            try
            {
                Type harmonyType = null;

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
                        break;
                    }
                }

                if (harmonyType == null)
                {
                    return patches;
                }

                var getAllPatchedMethods = harmonyType.GetMethod("GetAllPatchedMethods", PublicStatic);
                var getPatchInfo = harmonyType.GetMethod("GetPatchInfo", PublicStatic, null, new[] { typeof(MethodBase) }, null);

                if (getAllPatchedMethods == null || getPatchInfo == null)
                {
                    return patches;
                }

                if (!(getAllPatchedMethods.Invoke(null, null) is IEnumerable methods))
                {
                    return patches;
                }

                foreach (var methodObj in methods)
                {
                    if (!(methodObj is MethodBase method))
                    {
                        continue;
                    }

                    try
                    {
                        var patchesObj = getPatchInfo.Invoke(null, new object[] { method });
                        if (patchesObj == null)
                        {
                            continue;
                        }

                        var patchesType = patchesObj.GetType();
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Prefixes", method, PatchType.Prefix);
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Postfixes", method, PatchType.Postfix);
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Transpilers", method, PatchType.Transpiler);
                        AddPatchesFromProperty(patches, patchesType, patchesObj, "Finalizers", method, PatchType.Finalizer);
                    }
                    catch (Exception)
                    {
                        // Skip individual method inspection failures
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if Harmony 2 is not available or reflection fails
            }

            return patches;
        }

        private static void AddPatchesFromProperty(
            List<PatchInspectionInfo> destination,
            Type patchesType,
            object patchesObj,
            string propertyName,
            MethodBase targetMethod,
            PatchType patchType)
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
