// SPDX-License-Identifier: MIT

#pragma warning disable CA1859 // List vs IEnumerable for performance
#pragma warning disable CA1031 // General exception catch
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using HarmonyLib;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    [ExcludeFromCodeCoverage(Justification = "Requires live Harmony patch state")]
    public sealed class ExpectedPatchSource : IExpectedPatchSource
    {
        private static readonly object s_assemblyScanLock = new object();
        private static int s_suppressedExceptionCount;
        private static readonly HashSet<string> s_frameworkAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mscorlib", "System", "System.Core", "Microsoft.Xna.Framework", "Microsoft.Xna.Framework.Game",
            "UnityEditor", "UnityEngine", "UnityEngine.CoreModule", "UnityEngine.UI", "UnityEngine.Networking",
            "0Harmony", "0Harmony12", "0Harmony109", "BepInEx", "BepInEx.Core", "LobotomyBaseModLib", "BepInEx.ConfigurationManager",
            "Assembly-CSharp",
        };

        private const BindingFlags StaticMethodFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceFieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo)
        {
            if (debugInfo == null) throw new ArgumentNullException(nameof(debugInfo));

            var expectedPatches = new List<ExpectedPatchInfo>();
            var reflectionScannedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var harmonyPatchAttributeType = ResolveHarmonyPatchAttributeType(debugInfo);

            // Phase 1: Reflection scan ALL non-framework assemblies
            debugInfo.Add("=== Phase 1: Reflection-based scan ===");
            lock (s_assemblyScanLock)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    var assemblyName = assembly.GetName();
                    if (assemblyName == null || ShouldSkipAssembly(assemblyName.Name))
                    {
                        continue;
                    }

                    try
                    {
                        var reflectionPatches = ScanAssemblyViaReflection(assembly, assemblyName.Name, harmonyPatchAttributeType, debugInfo);
                        if (reflectionPatches.Count > 0)
                        {
                            expectedPatches.AddRange(reflectionPatches);
                            reflectionScannedAssemblies.Add(assemblyName.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(debugInfo, "Reflection: error scanning assembly " + assemblyName.Name, ex);
                    }
                }
            }

            var reflectionCount = expectedPatches.Count;
            debugInfo.Add("Reflection: total patches found: " + reflectionCount.ToString(CultureInfo.InvariantCulture));
            debugInfo.Add("Reflection: assemblies with patches: " + reflectionScannedAssemblies.Count.ToString(CultureInfo.InvariantCulture));

            // Phase 2: Source scan as fallback for LobotomyCorporationMods.* that reflection missed
            debugInfo.Add("=== Phase 2: Source-scan fallback ===");
            var sourcePatches = ScanSourceFallback(reflectionScannedAssemblies, debugInfo);
            expectedPatches.AddRange(sourcePatches);

            // Phase 3: runtime patch-state fallback for assemblies that still have no expected patches
            debugInfo.Add("=== Phase 3: Runtime fallback scan ===");
            var runtimeFallbackPatches = ScanRuntimeFallback(expectedPatches, debugInfo);
            expectedPatches.AddRange(runtimeFallbackPatches);

            debugInfo.Add("Source fallback: " + sourcePatches.Count.ToString(CultureInfo.InvariantCulture) + " additional patches found");
            debugInfo.Add("Runtime fallback: " + runtimeFallbackPatches.Count.ToString(CultureInfo.InvariantCulture) + " additional patches found");
            debugInfo.Add("Totals: reflection=" + reflectionCount.ToString(CultureInfo.InvariantCulture) +
                ", source=" + sourcePatches.Count.ToString(CultureInfo.InvariantCulture) +
                ", runtime=" + runtimeFallbackPatches.Count.ToString(CultureInfo.InvariantCulture) +
                ", combined=" + expectedPatches.Count.ToString(CultureInfo.InvariantCulture));

            if (s_suppressedExceptionCount > 0)
            {
                debugInfo.Add("WARNING: " + s_suppressedExceptionCount.ToString(CultureInfo.InvariantCulture) + " exceptions were suppressed during patch detection. Review debug output for details.");
            }

            return expectedPatches;
        }

        private static void LogException(IList<string> debugInfo, string context, Exception ex, bool includeStackTrace = true)
        {
            Interlocked.Increment(ref s_suppressedExceptionCount);
            var message = "Exception: " + context;
            if (includeStackTrace && ex.StackTrace != null)
            {
                message += " - " + ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace;
            }
            else
            {
                message += " - " + ex.GetType().Name + ": " + ex.Message;
            }

            debugInfo.Add(message);
        }

        private static Type ResolveHarmonyPatchAttributeType(IList<string> debugInfo)
        {
            try
            {
                var harmonyPatchType = typeof(HarmonyPatch);
                debugInfo.Add("Reflection: HarmonyPatch attribute type resolved: " + harmonyPatchType.FullName + " from " + harmonyPatchType.Assembly.GetName().Name);
                return harmonyPatchType;
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Reflection: failed to resolve HarmonyPatch attribute type", ex, false);
                return null;
            }
        }

        private static List<ExpectedPatchInfo> ScanAssemblyViaReflection(Assembly assembly, string assemblyName, Type harmonyPatchAttributeType, IList<string> debugInfo)
        {
            var patches = new List<ExpectedPatchInfo>();
            var types = SafeGetTypes(assembly, assemblyName, debugInfo);

            debugInfo.Add("Reflection: scanning " + assemblyName + " (" + types.Length.ToString(CultureInfo.InvariantCulture) + " types)");

            foreach (var type in types)
            {
                if (type == null)
                {
                    continue;
                }

                try
                {
                    var typePatches = ScanTypeViaReflection(type, assemblyName, harmonyPatchAttributeType, debugInfo);
                    patches.AddRange(typePatches);
                }
                catch (Exception ex)
                {
                    var typeName = GetTypeDisplayName(type);
                    LogException(debugInfo, "Reflection: error on type " + typeName, ex);
                }
            }

            debugInfo.Add("Reflection: " + assemblyName + ": " + patches.Count.ToString(CultureInfo.InvariantCulture) + " expected patches found");
            return patches;
        }

        private static List<ExpectedPatchInfo> ScanTypeViaReflection(Type type, string assemblyName, Type harmonyPatchAttributeType, IList<string> debugInfo)
        {
            var patches = new List<ExpectedPatchInfo>();
            var typeName = GetTypeDisplayName(type);
            var typePatchAttributes = GetHarmonyPatchAttributes(type, harmonyPatchAttributeType, debugInfo);

            Type typeTargetDeclaringType;
            string typeTargetMethodName;
            MergePatchTargetFromAttributes(typePatchAttributes, out typeTargetDeclaringType, out typeTargetMethodName, debugInfo, typeName + " (type)");

            if (typePatchAttributes.Count > 0)
            {
                debugInfo.Add("Reflection: [HarmonyPatch] found on " + typeName + " target=" +
                    (typeTargetDeclaringType != null ? typeTargetDeclaringType.FullName : "null") + "." +
                    (typeTargetMethodName ?? "null"));
            }

            var methods = type.GetMethods(StaticMethodFlags);
            foreach (var method in methods)
            {
                var patchType = ClassifyPatchMethod(method);
                if (patchType == null)
                {
                    continue;
                }

                var methodPatchAttributes = GetHarmonyPatchAttributes(method, harmonyPatchAttributeType, debugInfo);
                if (typePatchAttributes.Count == 0 && methodPatchAttributes.Count == 0)
                {
                    // Avoid false positives from arbitrary static methods named Prefix/Postfix.
                    continue;
                }

                Type methodTargetDeclaringType;
                string methodTargetMethodName;
                MergePatchTargetFromAttributes(methodPatchAttributes, out methodTargetDeclaringType, out methodTargetMethodName, debugInfo, typeName + "." + method.Name + " (method)");

                var targetDeclaringType = methodTargetDeclaringType ?? typeTargetDeclaringType;
                var targetMethodName = !string.IsNullOrEmpty(methodTargetMethodName)
                    ? methodTargetMethodName
                    : typeTargetMethodName;

                if (targetDeclaringType == null || string.IsNullOrEmpty(targetMethodName))
                {
                    debugInfo.Add("Reflection: skipped patch method " + typeName + "." + method.Name +
                        " because target is incomplete (type=" +
                        (targetDeclaringType != null ? targetDeclaringType.FullName : "null") +
                        ", method=" + (targetMethodName ?? "null") + ")");
                    continue;
                }

                var targetTypeName = targetDeclaringType.FullName ?? targetDeclaringType.Name;
                patches.Add(new ExpectedPatchInfo(assemblyName, targetTypeName, targetMethodName, method.Name, patchType.Value));

                debugInfo.Add("Reflection: " + assemblyName + ": " + patchType.Value + " for " +
                    targetTypeName + "." + targetMethodName + " (patch method=" + typeName + "." + method.Name + ")");
            }

            return patches;
        }

        private static List<object> GetHarmonyPatchAttributes(MemberInfo memberInfo, Type harmonyPatchAttributeType, IList<string> debugInfo)
        {
            var patchAttributes = new List<object>();
            if (memberInfo == null)
            {
                return patchAttributes;
            }

            object[] allAttributes;
            try
            {
                allAttributes = memberInfo.GetCustomAttributes(true);
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Reflection: failed to inspect attributes on " + memberInfo.Name, ex);
                return patchAttributes;
            }

            foreach (var attribute in allAttributes)
            {
                if (attribute == null)
                {
                    continue;
                }

                var attributeType = attribute.GetType();
                if (harmonyPatchAttributeType != null && harmonyPatchAttributeType.IsAssignableFrom(attributeType))
                {
                    patchAttributes.Add(attribute);
                    continue;
                }

                // Compatibility fallback for Harmony variants where the runtime type is not assignable to HarmonyLib.HarmonyPatch.
                if (string.Equals(attributeType.Name, "HarmonyPatch", StringComparison.Ordinal))
                {
                    patchAttributes.Add(attribute);
                    debugInfo.Add("Reflection: using name-based HarmonyPatch detection for " + memberInfo.Name + " via " + attributeType.FullName);
                }
            }

            return patchAttributes;
        }

        private static void MergePatchTargetFromAttributes(IList<object> attributes, out Type targetDeclaringType, out string targetMethodName, IList<string> debugInfo, string context)
        {
            targetDeclaringType = null;
            targetMethodName = null;

            foreach (var attribute in attributes)
            {
                Type currentDeclaringType;
                string currentMethodName;
                if (!TryExtractPatchTarget(attribute, out currentDeclaringType, out currentMethodName, debugInfo, context))
                {
                    continue;
                }

                if (currentDeclaringType != null)
                {
                    targetDeclaringType = currentDeclaringType;
                }

                if (!string.IsNullOrEmpty(currentMethodName))
                {
                    targetMethodName = currentMethodName;
                }
            }
        }

        private static bool TryExtractPatchTarget(object patchAttribute, out Type declaringType, out string methodName, IList<string> debugInfo, string context)
        {
            declaringType = null;
            methodName = null;

            if (patchAttribute == null)
            {
                return false;
            }

            try
            {
                var patchAttributeType = patchAttribute.GetType();
                var infoField = patchAttributeType.GetField("info", InstanceFieldFlags);
                if (infoField == null)
                {
                    debugInfo.Add("Reflection: HarmonyPatch attribute has no 'info' field on " + context + " (attribute type=" + patchAttributeType.FullName + ")");
                    return true;
                }

                var infoValue = infoField.GetValue(patchAttribute);
                if (infoValue == null)
                {
                    return true;
                }

                declaringType = GetMemberValueAsType(infoValue, "declaringType", "originalType", "type");
                methodName = GetMemberValueAsString(infoValue, "methodName", "name");

                var targetMethod = GetMemberValueAsMethodInfo(infoValue, "method", "originalMethod");
                if (targetMethod != null)
                {
                    if (declaringType == null)
                    {
                        declaringType = targetMethod.DeclaringType;
                    }

                    if (string.IsNullOrEmpty(methodName))
                    {
                        methodName = targetMethod.Name;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Reflection: failed to read HarmonyPatch target on " + context, ex);
                return false;
            }
        }

        private static Type GetMemberValueAsType(object source, params string[] memberNames)
        {
            foreach (var memberName in memberNames)
            {
                var memberValue = GetMemberValue(source, memberName);
                if (memberValue is Type typeValue)
                {
                    return typeValue;
                }
            }

            return null;
        }

        private static string GetMemberValueAsString(object source, params string[] memberNames)
        {
            foreach (var memberName in memberNames)
            {
                var memberValue = GetMemberValue(source, memberName) as string;
                if (!string.IsNullOrEmpty(memberValue))
                {
                    return memberValue;
                }
            }

            return null;
        }

        private static MethodInfo GetMemberValueAsMethodInfo(object source, params string[] memberNames)
        {
            foreach (var memberName in memberNames)
            {
                var methodValue = GetMemberValue(source, memberName) as MethodInfo;
                if (methodValue != null)
                {
                    return methodValue;
                }
            }

            return null;
        }

        private static object GetMemberValue(object source, string memberName)
        {
            if (source == null || string.IsNullOrEmpty(memberName))
            {
                return null;
            }

            var sourceType = source.GetType();

            var field = sourceType.GetField(memberName, InstanceFieldFlags);
            if (field != null)
            {
                return field.GetValue(source);
            }

            var property = sourceType.GetProperty(memberName, InstanceFieldFlags);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(source, null);
            }

            return null;
        }

        private static PatchType? ClassifyPatchMethod(MethodInfo method)
        {
            // Check by standard method name convention
            switch (method.Name)
            {
                case "Prefix": return PatchType.Prefix;
                case "Postfix": return PatchType.Postfix;
                case "Transpiler": return PatchType.Transpiler;
                case "Finalizer": return PatchType.Finalizer;
            }

            // Check for Harmony attributes on the method
            try
            {
                var methodAttrs = method.GetCustomAttributes(false);
                foreach (var attr in methodAttrs)
                {
                    if (attr == null)
                    {
                        continue;
                    }

                    var attrTypeName = attr.GetType().Name;
                    switch (attrTypeName)
                    {
                        case "HarmonyPrefix": return PatchType.Prefix;
                        case "HarmonyPostfix": return PatchType.Postfix;
                        case "HarmonyTranspiler": return PatchType.Transpiler;
                        case "HarmonyFinalizer": return PatchType.Finalizer;
                    }
                }
            }
            catch
            {
                // Ignore attribute access errors
            }

            return null;
        }

        private static Type[] SafeGetTypes(Assembly assembly, string assemblyName, IList<string> debugInfo)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                Interlocked.Increment(ref s_suppressedExceptionCount);
                var loadedTypeCount = ex.Types != null ? ex.Types.Length : 0;
                debugInfo.Add("Reflection: ReflectionTypeLoadException in " + assemblyName +
                    ", loaded " + loadedTypeCount.ToString(CultureInfo.InvariantCulture) + " types (some failed)");
                if (ex.LoaderExceptions != null)
                {
                    foreach (var loaderEx in ex.LoaderExceptions)
                    {
                        if (loaderEx != null)
                        {
                            debugInfo.Add("  Loader exception: " + loaderEx.GetType().Name + ": " + loaderEx.Message);
                            if (loaderEx.StackTrace != null)
                            {
                                debugInfo.Add("    " + loaderEx.StackTrace.Replace("\n", "\n    "));
                            }
                        }
                    }
                }

                return ex.Types ?? new Type[0];
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Reflection: failed to get types from " + assemblyName, ex);
                return new Type[0];
            }
        }

        private static List<ExpectedPatchInfo> ScanRuntimeFallback(IList<ExpectedPatchInfo> existingExpectedPatches, IList<string> debugInfo)
        {
            var fallbackPatches = new List<ExpectedPatchInfo>();
            var assembliesWithExpected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var knownPatchKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var expectedPatch in existingExpectedPatches)
            {
                if (expectedPatch == null)
                {
                    continue;
                }

                assembliesWithExpected.Add(expectedPatch.PatchAssembly);
                knownPatchKeys.Add(BuildExpectedPatchKey(expectedPatch.PatchAssembly, expectedPatch.TargetType, expectedPatch.TargetMethod, expectedPatch.PatchMethod, expectedPatch.PatchType));
            }

            try
            {
                lock (s_assemblyScanLock)
                {
                    foreach (var targetMethod in Harmony.GetAllPatchedMethods())
                    {
                        if (targetMethod == null)
                        {
                            continue;
                        }

                        var patchInfo = Harmony.GetPatchInfo(targetMethod);
                        if (patchInfo == null)
                        {
                            continue;
                        }

                        AddRuntimeFallbackPatches(fallbackPatches, knownPatchKeys, assembliesWithExpected, patchInfo.Prefixes, targetMethod, PatchType.Prefix, debugInfo);
                        AddRuntimeFallbackPatches(fallbackPatches, knownPatchKeys, assembliesWithExpected, patchInfo.Postfixes, targetMethod, PatchType.Postfix, debugInfo);
                        AddRuntimeFallbackPatches(fallbackPatches, knownPatchKeys, assembliesWithExpected, patchInfo.Transpilers, targetMethod, PatchType.Transpiler, debugInfo);
                        AddRuntimeFallbackPatches(fallbackPatches, knownPatchKeys, assembliesWithExpected, patchInfo.Finalizers, targetMethod, PatchType.Finalizer, debugInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Runtime fallback: unable to inspect active Harmony patch state", ex);
            }

            return fallbackPatches;
        }

        private static void AddRuntimeFallbackPatches(
            IList<ExpectedPatchInfo> destination,
            HashSet<string> knownPatchKeys,
            HashSet<string> assembliesWithExpected,
            IEnumerable<Patch> source,
            MethodBase targetMethod,
            PatchType patchType,
            IList<string> debugInfo)
        {
            if (source == null || targetMethod == null)
            {
                return;
            }

            foreach (var patch in source)
            {
                if (patch == null || patch.PatchMethod == null || patch.PatchMethod.DeclaringType == null)
                {
                    continue;
                }

                var patchAssemblyName = patch.PatchMethod.DeclaringType.Assembly.GetName().Name;
                if (string.IsNullOrEmpty(patchAssemblyName) || ShouldSkipAssembly(patchAssemblyName))
                {
                    continue;
                }

                if (assembliesWithExpected.Contains(patchAssemblyName))
                {
                    continue;
                }

                var targetType = targetMethod.DeclaringType != null
                    ? (targetMethod.DeclaringType.FullName ?? targetMethod.DeclaringType.Name)
                    : "Unknown";
                var targetMethodName = targetMethod.Name;
                var patchMethodName = patch.PatchMethod.Name;
                var patchKey = BuildExpectedPatchKey(patchAssemblyName, targetType, targetMethodName, patchMethodName, patchType);

                if (knownPatchKeys.Contains(patchKey))
                {
                    continue;
                }

                knownPatchKeys.Add(patchKey);
                var expectedPatch = new ExpectedPatchInfo(patchAssemblyName, targetType, targetMethodName, patchMethodName, patchType);
                destination.Add(expectedPatch);

                var patchDeclaringTypeName = patch.PatchMethod.DeclaringType.FullName ?? patch.PatchMethod.DeclaringType.Name;
                debugInfo.Add("Runtime fallback: " + patchAssemblyName + ": " + patchType + " for " + targetType + "." + targetMethodName +
                    " (patch method=" + patchDeclaringTypeName + "." + patchMethodName + ")");
            }
        }

        private static string BuildExpectedPatchKey(string assemblyName, string targetType, string targetMethod, string patchMethod, PatchType patchType)
        {
            return assemblyName + "|" + targetType + "|" + targetMethod + "|" + patchMethod + "|" + patchType;
        }

        // Phase 2: Source-scan fallback for LobotomyCorporationMods.* that reflection didn't find
        private static List<ExpectedPatchInfo> ScanSourceFallback(HashSet<string> reflectionScannedAssemblies, IList<string> debugInfo)
        {
            var expectedPatches = new List<ExpectedPatchInfo>();
            var scannedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            lock (s_assemblyScanLock)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    var assemblyName = assembly.GetName();
                    if (assemblyName == null || ShouldSkipAssembly(assemblyName.Name))
                    {
                        continue;
                    }

                    if (reflectionScannedAssemblies.Contains(assemblyName.Name))
                    {
                        continue;
                    }

                    try
                    {
                        var location = assembly.Location;
                        if (string.IsNullOrEmpty(location))
                        {
                            continue;
                        }

                        var dir = Path.GetDirectoryName(location);
                        if (string.IsNullOrEmpty(dir) || scannedDirectories.Contains(dir))
                        {
                            continue;
                        }

                        if (!IsKnownModAssembly(assemblyName.Name))
                        {
                            continue;
                        }

                        debugInfo.Add("Source fallback: scanning assembly dir for " + assemblyName.Name);
                        expectedPatches.AddRange(ScanModAssemblyForHarmonyPatches(dir, assemblyName.Name, debugInfo));
                        scannedDirectories.Add(dir);
                    }
                    catch (Exception ex)
                    {
                        LogException(debugInfo, "Source fallback: error assembly " + assemblyName.Name, ex);
                    }
                }
            }

            return expectedPatches;
        }

        private static bool ShouldSkipAssembly(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            if (s_frameworkAssemblies.Contains(name))
            {
                return true;
            }

            return name.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase) ||
                   name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                   name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownModAssembly(string name)
        {
            return name.StartsWith("LobotomyCorporationMods.", StringComparison.OrdinalIgnoreCase);
        }

        private static List<ExpectedPatchInfo> ScanModAssemblyForHarmonyPatches(string dir, string modName, IList<string> debugInfo)
        {
            var patches = new List<ExpectedPatchInfo>();
            if (!Directory.Exists(dir))
            {
                return patches;
            }

            try
            {
                var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        patches.AddRange(ScanFileForHarmonyPatches(file, modName));
                    }
                    catch (Exception ex)
                    {
                        LogException(debugInfo, "Error file " + Path.GetFileName(file), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(debugInfo, "Error dir " + dir, ex);
            }

            return patches;
        }

        private static List<ExpectedPatchInfo> ScanFileForHarmonyPatches(string filePath, string modName)
        {
            var patches = new List<ExpectedPatchInfo>();
            var content = File.ReadAllText(filePath);

            // Match [HarmonyPatch(...)] - improved pattern that handles:
            // - Nested parentheses in typeof(...) expressions
            // - Multi-line attributes with RegexOptions.Multiline
            // - Generic types like typeof(List<T>)
            //
            // KNOWN LIMITATIONS:
            // - Does not handle string literals containing ']' characters
            // - Does not handle complex nested generic types with nested angle brackets
            // - Does not handle nameof() with full namespaces correctly in all cases
            // - May produce false positives for similar attribute names
            //
            // For production use, consider using a proper C# parser (Microsoft.CodeAnalysis.CSharp)
            // which would require .NET Standard 2.0+ (not available in this .NET 3.5 project).
            //
            // When "missing patch" warnings appear, manually review the HarmonyPatch declarations
            // to verify they are correctly detected.
            var matches = Regex.Matches(content, @"\[\s*HarmonyPatch\s*\(([^)]*(?:\([^)]*\)[^)]*)*)\)\s*\]", RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                var args = match.Groups[1].Value;
                string targetType = null;
                string targetMethod = null;

                var typeMatch = Regex.Match(args, @"typeof\s*\(([^)]+)\)");
                if (typeMatch.Success)
                {
                    targetType = typeMatch.Groups[1].Value.Trim();
                }

                // Handle both "Method", nameof(...), and constants
                var methodNameMatch = Regex.Match(args, @"""([^""]+)""|nameof\s*\((?:[^.]+\.)?([^)]+)\)|,\s*([^,\s)]+)");
                if (methodNameMatch.Success)
                {
                    targetMethod = (methodNameMatch.Groups[1].Value + methodNameMatch.Groups[2].Value + methodNameMatch.Groups[3].Value).Trim();
                    if (targetMethod.Contains("."))
                    {
                        var split = targetMethod.Split('.');
                        targetMethod = split.Length > 0 ? split[split.Length - 1] : targetMethod;
                    }
                }

                if (string.IsNullOrEmpty(targetType))
                {
                    targetType = FindClassLevelTargetType(content, match.Index);
                }

                if (string.IsNullOrEmpty(targetType) || string.IsNullOrEmpty(targetMethod))
                {
                    continue;
                }

                var afterAttribute = content.Substring(match.Index + match.Length);

                // 1. Check for class declaration
                var classMatch = Regex.Match(afterAttribute, @"^\s*(?:\[[^\]]+\]\s*)*(?:public|internal|private)?\s*(?:sealed|abstract)?\s*static\s+class\s+([^\s{]+)");
                if (classMatch.Success)
                {
                    var openBrace = afterAttribute.IndexOf('{');
                    if (openBrace == -1)
                    {
                        continue;
                    }

                    var classBody = afterAttribute.Substring(openBrace);
                    var methodMatches = Regex.Matches(classBody, @"(?:public|internal|private)?\s+static\s+[^\s(]+\s+(Prefix|Postfix|Transpiler|Finalizer)\s*\(");
                    foreach (Match patchMethodMatch in methodMatches)
                    {
                        var patchMethodName = patchMethodMatch.Groups[1].Value;
                        var patchType = patchMethodName == "Prefix"
                            ? PatchType.Prefix
                            : patchMethodName == "Postfix"
                                ? PatchType.Postfix
                                : patchMethodName == "Transpiler"
                                    ? PatchType.Transpiler
                                    : PatchType.Finalizer;

                        patches.Add(new ExpectedPatchInfo(modName, targetType, targetMethod, patchMethodName, patchType));
                    }

                    continue;
                }

                // 2. Check for method declaration
                var methodMatch = Regex.Match(afterAttribute, @"^\s*(?:\[[^\]]+\]\s*)*(?:public|internal|private)?\s+static\s+([^\s(]+)\s+([^\s(]+)\s*\(");
                if (methodMatch.Success)
                {
                    var methodName = methodMatch.Groups[2].Value;
                    var returnType = methodMatch.Groups[1].Value;
                    var patchType = returnType == "void" ? PatchType.Prefix : PatchType.Postfix;
                    patches.Add(new ExpectedPatchInfo(modName, targetType, targetMethod, methodName, patchType));
                }
            }

            return patches;
        }

        private static string FindClassLevelTargetType(string content, int index)
        {
            var before = content.Substring(0, index);
            var pattern = @"\[\s*HarmonyPatch\s*\(\s*typeof\s*\(([^)]+)\)\s*\)\s*\]\s*(?:\[[^\]]+\]\s*)*(?:public|internal|private)?\s*(?:sealed|abstract)?\s*static\s+class";
            var matches = Regex.Matches(before, pattern);
            if (matches.Count == 0)
            {
                return null;
            }

            return matches[matches.Count - 1].Groups[1].Value.Trim();
        }

        private static string GetTypeDisplayName(Type type)
        {
            if (type == null)
            {
                return "<unknown>";
            }

            try
            {
                return type.FullName ?? type.Name;
            }
            catch
            {
                return "<unknown>";
            }
        }
    }
}
