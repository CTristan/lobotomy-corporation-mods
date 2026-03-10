// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace SetupExternal
{
    /// <summary>
    /// AssemblyRetargeter - Corrects mscorlib version mismatches in game DLLs.
    /// 
    /// Basemod-patched DLLs sometimes contain multiple mscorlib references (v2.0.0.0 and v4.0.0.0),
    /// which causes MSBuild to fail with warning MSB3258 and CS0246 errors.
    /// This class consolidates all mscorlib references to v2.0.0.0.
    /// </summary>
    public static class AssemblyRetargeter
    {
        private static readonly Version TargetVersion = new(2, 0, 0, 0);

        /// <summary>
        /// Retargets the specified DLL to use mscorlib v2.0.0.0.
        /// </summary>
        /// <param name="dllPath">The path to the DLL to retarget.</param>
        /// <returns>True if the DLL was modified; otherwise, false.</returns>
        public static bool Retarget(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                Program.DebugLog($"[Retarget] File not found: {dllPath}");
                return false;
            }

            try
            {
                var fileName = Path.GetFileName(dllPath);
                Program.DebugLog($"[Retarget] Starting retarget check for {fileName}");

                // Set up assembly resolver to find dependencies in the same directory
                DefaultAssemblyResolver resolver = new();
                resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath));

                // Use ReaderParameters to read the assembly
                // We use ReadingMode.Immediate to avoid file locks after reading
                ReaderParameters readerParameters = new()
                {
                    ReadingMode = ReadingMode.Immediate,
                    AssemblyResolver = resolver
                };

                var modified = false;
                using (AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dllPath, readerParameters))
                {
                    List<AssemblyNameReference> mscorlibRefs = [.. assembly.MainModule.AssemblyReferences.Where(r => r.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))];

                    if (mscorlibRefs.Count == 0)
                    {
                        Program.DebugLog($"[Retarget] No mscorlib reference found in {fileName}");
                        return false;
                    }

                    Program.DebugLog($"[Retarget] Found {mscorlibRefs.Count} mscorlib reference(s) in {fileName}");
                    foreach (var asmRef in mscorlibRefs)
                    {
                        Program.DebugLog($"[Retarget]   - mscorlib {asmRef.Version}");
                    }

                    // If there are multiple mscorlib references, remove all but one
                    if (mscorlibRefs.Count > 1)
                    {
                        Program.DebugLog($"[Retarget] Found {mscorlibRefs.Count} mscorlib references in {fileName}. Consolidating...");

                        // Log diagnostic information before the fix
                        var typeRefsWithWrongScope = 0;
                        List<TypeReference> allTypeRefs = [.. assembly.MainModule.GetTypeReferences()];
                        foreach (var typeRef in allTypeRefs)
                        {
                            if (typeRef.Scope != null && !mscorlibRefs.Contains(typeRef.Scope))
                            {
                                typeRefsWithWrongScope++;
                            }
                        }
                        if (typeRefsWithWrongScope > 0)
                        {
                            Program.DebugLog($"[Retarget] Diagnostic: Found {typeRefsWithWrongScope} TypeRefs scoped to non-mscorlib assemblies");
                        }

                        // Log some specific TypeRef scopes for debugging
                        var systemObjectTypeRef = allTypeRefs
                            .FirstOrDefault(tr => tr.FullName == "System.Object");
                        if (systemObjectTypeRef != null)
                        {
                            var scopeName = systemObjectTypeRef.Scope is AssemblyNameReference asmRef
                                ? $"{asmRef.Name} {asmRef.Version}"
                                : systemObjectTypeRef.Scope?.GetType().Name ?? "null";
                            Program.DebugLog($"[Retarget] System.Object TypeRef Scope: {scopeName}");
                        }

                        // Log the TypeRef at index 464 (token 0x010001d0 from the error message)
                        if (allTypeRefs.Count > 464)
                        {
                            var typeRef464 = allTypeRefs[464];
                            var scopeName464 = typeRef464.Scope is AssemblyNameReference asmRef
                                ? $"{asmRef.Name} {asmRef.Version}"
                                : typeRef464.Scope?.GetType().Name ?? "null";
                            Program.DebugLog($"[Retarget] TypeRef at index 464 (token 0x010001d0): {typeRef464.FullName}, Scope: {scopeName464}");
                        }

                        // Keep the first one and remove the rest
                        var keepRef = mscorlibRefs[0];
                        List<AssemblyNameReference> refsToRemove = [.. mscorlibRefs.Skip(1)];

                        var typeRescoped = 0;
                        var memberRescoped = 0;
                        var exportedTypeRescoped = 0;

                        // Re-scope TypeRefs before removing duplicate mscorlib references
                        foreach (var removeRef in refsToRemove)
                        {
                            // Re-scope TypeRefs - compare by assembly name and version
                            foreach (var typeRef in assembly.MainModule.GetTypeReferences())
                            {
                                if (typeRef.Scope is AssemblyNameReference scopeAsmRef &&
                                    scopeAsmRef.Name == removeRef.Name &&
                                    scopeAsmRef.Version == removeRef.Version)
                                {
                                    typeRef.Scope = keepRef;
                                    typeRescoped++;
                                }
                            }

                            // Re-scope MemberRefs
                            foreach (var memberRef in assembly.MainModule.GetMemberReferences())
                            {
                                if (memberRef.DeclaringType?.Scope is AssemblyNameReference scopeAsmRef &&
                                    scopeAsmRef.Name == removeRef.Name &&
                                    scopeAsmRef.Version == removeRef.Version)
                                {
                                    memberRef.DeclaringType.Scope = keepRef;
                                    memberRescoped++;
                                }
                            }

                            // Re-scope ExportedTypes
                            foreach (var exportedType in assembly.MainModule.ExportedTypes)
                            {
                                if (exportedType.Scope is AssemblyNameReference scopeAsmRef &&
                                    scopeAsmRef.Name == removeRef.Name &&
                                    scopeAsmRef.Version == removeRef.Version)
                                {
                                    exportedType.Scope = keepRef;
                                    exportedTypeRescoped++;
                                }
                            }

                            // Remove the duplicate reference
                            _ = assembly.MainModule.AssemblyReferences.Remove(removeRef);
                        }

                        // Log the results of the re-scoping
                        if (typeRescoped > 0 || memberRescoped > 0 || exportedTypeRescoped > 0)
                        {
                            Program.DebugLog($"[Retarget] Re-scoped metadata: {typeRescoped} TypeRefs, {memberRescoped} MemberRefs, {exportedTypeRescoped} ExportedTypes");
                        }

                        // Verify System.Object TypeRef scope after re-scoping
                        var systemObjectTypeRefAfter = assembly.MainModule.GetTypeReferences()
                            .FirstOrDefault(tr => tr.FullName == "System.Object");
                        if (systemObjectTypeRefAfter != null)
                        {
                            var scopeNameAfter = systemObjectTypeRefAfter.Scope is AssemblyNameReference asmRef
                                ? $"{asmRef.Name} {asmRef.Version}"
                                : systemObjectTypeRefAfter.Scope?.GetType().Name ?? "null";
                            Program.DebugLog($"[Retarget] System.Object TypeRef Scope AFTER re-scoping: {scopeNameAfter}");
                        }

                        // Verify System.String TypeRef scope after re-scoping (token 0x010001d0)
                        var systemStringTypeRefAfter = assembly.MainModule.GetTypeReferences()
                            .FirstOrDefault(tr => tr.FullName == "System.String");
                        if (systemStringTypeRefAfter != null)
                        {
                            var scopeNameAfter = systemStringTypeRefAfter.Scope is AssemblyNameReference asmRef
                                ? $"{asmRef.Name} {asmRef.Version}"
                                : systemStringTypeRefAfter.Scope?.GetType().Name ?? "null";
                            Program.DebugLog($"[Retarget] System.String TypeRef Scope AFTER re-scoping: {scopeNameAfter}");
                        }

                        // Ensure the remaining one has the correct version
                        if (keepRef.Version != TargetVersion)
                        {
                            keepRef.Version = TargetVersion;
                        }

                        modified = true;
                    }
                    else
                    {
                        // Single mscorlib reference, check if it needs a version downgrade
                        var mscorlibRef = mscorlibRefs[0];
                        if (mscorlibRef.Version != TargetVersion)
                        {
                            Program.DebugLog($"[Retarget] Downgrading mscorlib reference in {fileName} from {mscorlibRef.Version} to {TargetVersion}");
                            mscorlibRef.Version = TargetVersion;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        var tempPath = dllPath + ".tmp";
                        try
                        {
                            assembly.Write(tempPath);
                        }
                        catch
                        {
                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }
                            throw;
                        }
                    }
                }

                if (modified)
                {
                    var tempPath = dllPath + ".tmp";
                    if (File.Exists(tempPath))
                    {
                        File.Delete(dllPath);
                        File.Move(tempPath, dllPath);

                        Program.DebugLog($"[Retarget] Successfully retargeted {Path.GetFileName(dllPath)}");
                        return true;
                    }
                }

                Program.DebugLog($"[Retarget] No changes needed for {Path.GetFileName(dllPath)}");
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retargeting {dllPath}: {ex.Message}");
                Program.DebugLog($"[Retarget] Exception: {ex}");
                return false;
            }
        }
    }
}
