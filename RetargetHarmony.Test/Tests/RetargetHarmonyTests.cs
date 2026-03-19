// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using Mono.Cecil;
using Xunit;
using static RetargetHarmony.RetargetHarmony;

namespace RetargetHarmony.Test.Tests
{
    public sealed class RetargetHarmonyTests(ITestOutputHelper output)
    {
        private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        private static readonly string ExternalDir = Path.Combine(RepoRoot, "external");
        private static readonly string ManagedDir = Path.Combine(ExternalDir, "LobotomyCorp_Data", "Managed");

        private readonly ITestOutputHelper _output = output;

        [Fact]
        public void ExternalDirectoryExists()
        {
            _output.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
            _output.WriteLine($"RepoRoot: {RepoRoot}");
            _output.WriteLine($"ExternalDir: {ExternalDir}");

            _ = Directory.Exists(ExternalDir).Should().BeTrue("external directory should exist");
            _ = File.Exists(GetManagedAssemblyPath("Assembly-CSharp.dll")).Should().BeTrue("Assembly-CSharp.dll should exist in external directory");
            _ = File.Exists(GetManagedAssemblyPath("LobotomyBaseModLib.dll")).Should().BeTrue("LobotomyBaseModLib.dll should exist in external directory");
        }

        [Fact]
        public void Patch_ThrowsArgumentNullException_WhenAssemblyIsNull()
        {
            Action act = () => Patch(null);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("asm");
        }

        [Fact]
        public void Patch_RetargetsHarmonyReference_InAssemblyCSharp()
        {
            using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));

            Patch(assemblyDefinition);

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after patching");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should be retargeted to 0Harmony109");
        }

        [Fact]
        public void Patch_RetargetsHarmonyReference_InLobotomyBaseModLib()
        {
            using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));

            Patch(assemblyDefinition);

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after patching");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should be retargeted to 0Harmony109");
        }

        [Fact]
        public void Patch_DoesNotModifyOtherReferences_InAssemblyCSharp()
        {
            using AssemblyDefinition originalAssembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
            List<string> originalRefs = [.. originalAssembly.MainModule.AssemblyReferences.Select(r => r.Name)];

            using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));

            Patch(assemblyDefinition);
            List<string> patchedRefs = [.. assemblyDefinition.MainModule.AssemblyReferences.Select(r => r.Name)];

            List<string> nonHarmonyOriginalRefs = [.. originalRefs.Where(r => r is not "0Harmony" and not "0Harmony109")];
            List<string> nonHarmonyPatchedRefs = [.. patchedRefs.Where(r => r is not "0Harmony" and not "0Harmony109")];

            _ = nonHarmonyPatchedRefs.Should().BeEquivalentTo(nonHarmonyOriginalRefs,
                "non-Harmony references should not be modified");
        }

        [Fact]
        public void Patch_DoesNotModifyOtherReferences_InLobotomyBaseModLib()
        {
            using AssemblyDefinition originalAssembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));
            List<string> originalRefs = [.. originalAssembly.MainModule.AssemblyReferences.Select(r => r.Name)];

            using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));

            Patch(assemblyDefinition);
            List<string> patchedRefs = [.. assemblyDefinition.MainModule.AssemblyReferences.Select(r => r.Name)];

            List<string> nonHarmonyOriginalRefs = [.. originalRefs.Where(r => r is not "0Harmony" and not "0Harmony109")];
            List<string> nonHarmonyPatchedRefs = [.. patchedRefs.Where(r => r is not "0Harmony" and not "0Harmony109")];

            _ = nonHarmonyPatchedRefs.Should().BeEquivalentTo(nonHarmonyOriginalRefs,
                "non-Harmony references should not be modified");
        }

        [Fact]
        public void Patch_Idempotent_WhenAlreadyRetargetedTo0Harmony109()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("0Harmony109");

            Patch(assemblyDefinition);
            Patch(assemblyDefinition);

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should still be exactly one Harmony reference");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should remain 0Harmony109");
        }

        [Fact]
        public void Patch_RemovesDuplicateHarmonyReferences()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony", "0Harmony");

            Patch(assemblyDefinition);

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after deduplication");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be retargeted to 0Harmony109");
        }

        [Fact]
        public void Patch_ConsolidatesMixedHarmonyReferences()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony109", "0Harmony");

            Patch(assemblyDefinition);

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after consolidation");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be 0Harmony109");
        }

        [Fact]
        public void Patch_DoesNothing_WhenNoHarmonyReference()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("mscorlib");

            var exception = Record.Exception(() => Patch(assemblyDefinition));

            _ = exception.Should().BeNull("patching an assembly without Harmony reference should not throw");

            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);
            _ = harmonyRefs.Should().BeEmpty("no Harmony references should be added");
        }

        [Fact]
        public void TargetDLLs_ReturnsExpectedAssemblies()
        {
            List<string> targetDlls = [.. TargetDLLs];

            _ = targetDlls.Should().HaveCount(2, "there should be 2 target DLLs");
            _ = targetDlls.Should().Contain("Assembly-CSharp.dll");
            _ = targetDlls.Should().Contain("LobotomyBaseModLib.dll");
        }

        [Fact]
        public void Finish_MethodExists()
        {
            // Verify Finish() method exists via reflection (BepInEx preloader contract)
            var finishMethod = typeof(RetargetHarmony).GetMethod("Finish", BindingFlags.Public | BindingFlags.Static);

            _ = finishMethod.Should().NotBeNull("Finish() method should exist for BepInEx preloader lifecycle");
            _ = finishMethod.GetParameters().Should().BeEmpty("Finish() should take no parameters");
        }

        [Fact]
        public void BaseModsPath_ComputesCorrectPath()
        {
            // Verify BaseModsPath computes a valid path structure
            var baseModsPath = BaseModsPath;

            _ = baseModsPath.Should().NotBeNullOrEmpty("BaseModsPath should not be empty");
            _ = baseModsPath.Should().EndWith("BaseMods", "BaseModsPath should end with BaseMods");
            _ = baseModsPath.Should().Contain("LobotomyCorp_Data", "BaseModsPath should contain LobotomyCorp_Data");
        }

        [Fact]
        public void PatchBaseModsEnabled_DefaultsToFalse()
        {
            // Clear any override and config
            PatchBaseModsOverride = false;
            ClearPatchBaseModsConfig();

            // Without config initialized, should default to false
            _ = PatchBaseModsEnabled.Should().BeFalse("by default PatchBaseMods should be disabled");
        }

        [Fact]
        public void PatchBaseModsEnabled_ReturnsTrue_WhenOverrideIsSet()
        {
            // Set override to true
            PatchBaseModsOverride = true;

            try
            {
                _ = PatchBaseModsEnabled.Should().BeTrue("when override is set, PatchBaseModsEnabled should return true");
            }
            finally
            {
                // Clean up
                PatchBaseModsOverride = false;
            }
        }

        [Fact]
        public void TargetDLLs_ExcludesBaseModsDlls_WhenPatchBaseModsDisabled()
        {
            // Ensure patch base mods is disabled
            PatchBaseModsOverride = false;
            ClearPatchBaseModsConfig();

            List<string> targetDlls = [.. TargetDLLs];

            // Should only contain the Managed DLLs
            _ = targetDlls.Should().HaveCount(2, "only Managed DLLs should be included when PatchBaseMods is disabled");
            _ = targetDlls.Should().Contain("Assembly-CSharp.dll");
            _ = targetDlls.Should().Contain("LobotomyBaseModLib.dll");
        }

        [Fact]
        public void Patch_WritesAuditLog_ForBaseModsDll()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_AuditLog_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                AuditLogDirectoryOverride = tempDir;
                using var assemblyDefinition = CreateSyntheticAssembly("0Harmony");
                // Rename to simulate a BaseMods DLL
                assemblyDefinition.Name.Name = "TestBaseMod";

                Patch(assemblyDefinition);

                var logPath = Path.Combine(tempDir, "patched_mods.log");
                _ = File.Exists(logPath).Should().BeTrue("audit log should be created");
                var entries = File.ReadAllLines(logPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                _ = entries.Should().HaveCount(1);
                _ = entries[0].Should().Contain("TestBaseMod.dll");
                _ = entries[0].Should().Contain(Path.Combine("LobotomyCorp_Data", "BaseMods"));
            }
            finally
            {
                AuditLogDirectoryOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void Patch_DoesNotWriteAuditLog_ForCoreAssemblies()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_AuditLog_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                AuditLogDirectoryOverride = tempDir;

                using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
                Patch(assemblyDefinition);

                var logPath = Path.Combine(tempDir, "patched_mods.log");
                _ = File.Exists(logPath).Should().BeFalse("audit log should not be created for core assemblies");
            }
            finally
            {
                AuditLogDirectoryOverride = null;
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void Patch_DeduplicatesAuditLogEntries()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_AuditLog_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                AuditLogDirectoryOverride = tempDir;

                // Patch same BaseMods DLL twice
                using var asm1 = CreateSyntheticAssembly("0Harmony");
                asm1.Name.Name = "TestBaseMod";
                Patch(asm1);

                // Reset references so it can be patched again
                using var asm2 = CreateSyntheticAssembly("0Harmony");
                asm2.Name.Name = "TestBaseMod";
                Patch(asm2);

                var logPath = Path.Combine(tempDir, "patched_mods.log");
                var entries = File.ReadAllLines(logPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                _ = entries.Should().HaveCount(1, "duplicate entries should not be added");
            }
            finally
            {
                AuditLogDirectoryOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void GameRootPath_ReturnsNonEmptyPath()
        {
            var gameRootPath = GameRootPath;

            _ = gameRootPath.Should().NotBeNullOrEmpty("GameRootPath should not be empty");
        }

        [Fact]
        public void RetargetHarmonyReferences_ReturnsTrue_WhenHarmonyReferenceRetargeted()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("0Harmony");

            var changed = RetargetHarmonyReferences(assemblyDefinition);

            _ = changed.Should().BeTrue("retargeting should report changes were made");
            var harmonyRefs = GetHarmonyReferences(assemblyDefinition);
            _ = harmonyRefs.Should().HaveCount(1);
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109");
        }

        [Fact]
        public void RetargetHarmonyReferences_ReturnsFalse_WhenNoHarmonyReference()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("mscorlib");

            var changed = RetargetHarmonyReferences(assemblyDefinition);

            _ = changed.Should().BeFalse("no Harmony references means no changes");
        }

        [Fact]
        public void RetargetHarmonyReferences_ReturnsFalse_WhenAlreadyRetargeted()
        {
            using var assemblyDefinition = CreateSyntheticAssembly("0Harmony109");

            var changed = RetargetHarmonyReferences(assemblyDefinition);

            _ = changed.Should().BeFalse("already retargeted assembly should report no changes");
        }

        [Fact]
        public void OnAssemblyLoadFile_RetargetsDllInMemory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_LoadFile_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                BaseModsPathOverride = tempDir;

                // Create a minimal DLL with a 0Harmony reference and write it to disk
                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                WriteSyntheticAssemblyToDisk(dllPath, "0Harmony");

                // Record original bytes
                var originalBytes = File.ReadAllBytes(dllPath);

                // Call the prefix
                Assembly? result = null;
                var shouldRunOriginal = OnAssemblyLoadFile(ref result, dllPath);

                // Prefix should skip the original (return false) and provide a result
                _ = shouldRunOriginal.Should().BeFalse("prefix should handle the load itself");
                _ = result.Should().NotBeNull("prefix should return a loaded assembly");

                // Verify the loaded assembly has retargeted references
                var referencedAssemblies = result.GetReferencedAssemblies();
                _ = referencedAssemblies.Should().Contain(r => r.Name == "0Harmony109",
                    "loaded assembly should reference 0Harmony109");

                // Verify original file on disk is UNCHANGED
                var bytesAfter = File.ReadAllBytes(dllPath);
                _ = bytesAfter.Should().BeEquivalentTo(originalBytes,
                    "original DLL on disk must not be modified");
            }
            finally
            {
                BaseModsPathOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnAssemblyLoadFile_PassesThrough_WhenNotInBaseMods()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_NotBaseMods_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                // Set BaseModsPath to a different directory
                BaseModsPathOverride = Path.Combine(Path.GetTempPath(), "SomeOtherDir_" + Guid.NewGuid().ToString("N"));

                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                WriteSyntheticAssemblyToDisk(dllPath, "0Harmony");

                Assembly? result = null;
                var shouldRunOriginal = OnAssemblyLoadFile(ref result, dllPath);

                _ = shouldRunOriginal.Should().BeTrue("prefix should pass through for non-BaseMods paths");
                _ = result.Should().BeNull("prefix should not set result when passing through");
            }
            finally
            {
                BaseModsPathOverride = null;
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Fact]
        public void OnAssemblyLoadFile_PassesThrough_WhenNoHarmonyReference()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_NoHarmony_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                BaseModsPathOverride = tempDir;

                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                WriteSyntheticAssemblyToDisk(dllPath, "mscorlib");

                Assembly? result = null;
                var shouldRunOriginal = OnAssemblyLoadFile(ref result, dllPath);

                _ = shouldRunOriginal.Should().BeTrue("prefix should pass through when no Harmony reference");
                _ = result.Should().BeNull("prefix should not set result when passing through");
            }
            finally
            {
                BaseModsPathOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnAssemblyLoadFile_PassesThrough_WhenHarmony2xReference()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_Harmony2x_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                BaseModsPathOverride = tempDir;

                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                WriteSyntheticAssemblyToDiskWithVersion(dllPath, new Version(2, 9, 0, 0), "0Harmony");

                Assembly? result = null;
                var shouldRunOriginal = OnAssemblyLoadFile(ref result, dllPath);

                _ = shouldRunOriginal.Should().BeTrue("prefix should pass through for Harmony 2.x references");
                _ = result.Should().BeNull("prefix should not set result when passing through");
            }
            finally
            {
                BaseModsPathOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void OnAssemblyLoadFile_PassesThrough_WhenAlreadyRetargeted()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_Already_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                BaseModsPathOverride = tempDir;

                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                WriteSyntheticAssemblyToDisk(dllPath, "0Harmony109");

                Assembly? result = null;
                var shouldRunOriginal = OnAssemblyLoadFile(ref result, dllPath);

                _ = shouldRunOriginal.Should().BeTrue("prefix should pass through when already retargeted");
                _ = result.Should().BeNull("prefix should not set result when passing through");
            }
            finally
            {
                BaseModsPathOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void TargetDLLs_NeverIncludesBaseMods()
        {
            // Regardless of PatchBaseModsEnabled, TargetDLLs should only return core assemblies
            PatchBaseModsOverride = true;

            try
            {
                List<string> targetDlls = [.. TargetDLLs];

                _ = targetDlls.Should().HaveCount(2, "TargetDLLs should always return exactly 2 core assemblies");
                _ = targetDlls.Should().Contain("Assembly-CSharp.dll");
                _ = targetDlls.Should().Contain("LobotomyBaseModLib.dll");
            }
            finally
            {
                PatchBaseModsOverride = false;
            }
        }

        [Fact]
        public void RetargetHarmonyReferences_SkipsHarmony2x_WhenVersionMajorIsTwo()
        {
            using var assemblyDefinition = CreateSyntheticAssemblyWithVersion(new Version(2, 9, 0, 0), "0Harmony");

            var changed = RetargetHarmonyReferences(assemblyDefinition);

            _ = changed.Should().BeFalse("Harmony 2.x references should not be retargeted");
            var refs = assemblyDefinition.MainModule.AssemblyReferences.Where(r => r.Name == "0Harmony").ToList();
            _ = refs.Should().HaveCount(1, "the original 0Harmony reference should be preserved");
            _ = refs[0].Version.Major.Should().Be(2, "the version should remain unchanged");
        }

        [Fact]
        public void RetargetHarmonyReferences_RetargetsHarmony1x_ButSkipsHarmony2x()
        {
            using var assemblyDefinition = CreateSyntheticAssemblyWithVersion(new Version(1, 0, 0, 0), "mscorlib");
            // Add a Harmony 1.x reference
            assemblyDefinition.MainModule.AssemblyReferences.Add(new AssemblyNameReference("0Harmony", new Version(1, 0, 9, 1)));
            // Add a Harmony 2.x reference (e.g., a BepInEx plugin using HarmonyX)
            assemblyDefinition.MainModule.AssemblyReferences.Add(new AssemblyNameReference("0Harmony", new Version(2, 9, 0, 0)));

            var changed = RetargetHarmonyReferences(assemblyDefinition);

            _ = changed.Should().BeTrue("the Harmony 1.x reference should be retargeted");
            var harmony109Refs = assemblyDefinition.MainModule.AssemblyReferences.Where(r => r.Name == "0Harmony109").ToList();
            _ = harmony109Refs.Should().HaveCount(1, "Harmony 1.x should be retargeted to 0Harmony109");
            var harmony2Refs = assemblyDefinition.MainModule.AssemblyReferences.Where(r => r.Name == "0Harmony" && r.Version.Major == 2).ToList();
            _ = harmony2Refs.Should().HaveCount(1, "Harmony 2.x reference should be preserved");
        }

        [Fact]
        public void BlockTryShim_ReturnsFalse_ForBaseModsPath()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTest_BlockTryShim_" + Guid.NewGuid().ToString("N"));
            _ = Directory.CreateDirectory(tempDir);

            try
            {
                BaseModsPathOverride = tempDir;

                var dllPath = Path.Combine(tempDir, "TestMod.dll");
                var result = BlockTryShim(dllPath);

                _ = result.Should().BeFalse("BlockTryShim should block TryShim for BaseMods paths");
            }
            finally
            {
                BaseModsPathOverride = null;
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void BlockTryShim_ReturnsTrue_ForNonBaseModsPath()
        {
            try
            {
                BaseModsPathOverride = Path.Combine(Path.GetTempPath(), "SomeOtherDir_" + Guid.NewGuid().ToString("N"));

                var result = BlockTryShim("/some/other/path/TestMod.dll");

                _ = result.Should().BeTrue("BlockTryShim should allow TryShim for non-BaseMods paths");
            }
            finally
            {
                BaseModsPathOverride = null;
            }
        }

        [Fact]
        public void BlockTryShim_ReturnsTrue_WhenPathIsNull()
        {
            var result = BlockTryShim(null);

            _ = result.Should().BeTrue("BlockTryShim should allow TryShim when path is null");
        }

        private static string GetManagedAssemblyPath(string fileName)
        {
            return Path.Combine(ManagedDir, fileName);
        }

        private static List<AssemblyNameReference> GetHarmonyReferences(AssemblyDefinition assemblyDefinition)
        {
            return [.. assemblyDefinition.MainModule.AssemblyReferences.Where(r => r.Name is "0Harmony" or "0Harmony109")];
        }

        private static AssemblyDefinition CreateSyntheticAssembly(params string[] referencedAssemblies)
        {
            return CreateSyntheticAssemblyWithVersion(new Version(1, 0, 0, 0), referencedAssemblies);
        }

        private static AssemblyDefinition CreateSyntheticAssemblyWithVersion(Version version, params string[] referencedAssemblies)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
            assembly.MainModule.AssemblyReferences.Clear();

            foreach (var refName in referencedAssemblies)
            {
                AssemblyNameReference reference = new(refName, version);
                assembly.MainModule.AssemblyReferences.Add(reference);
            }

            return assembly;
        }

        /// <summary>
        /// Writes a synthetic assembly to disk. Uses a MemoryStream intermediate step to avoid
        /// Mono.Cecil type resolution errors when writing Assembly-CSharp-based synthetics.
        /// </summary>
        private static void WriteSyntheticAssemblyToDisk(string filePath, params string[] referencedAssemblies)
        {
            WriteSyntheticAssemblyToDiskWithVersion(filePath, new Version(1, 0, 0, 0), referencedAssemblies);
        }

        private static void WriteSyntheticAssemblyToDiskWithVersion(string filePath, Version version, params string[] referencedAssemblies)
        {
            using var assembly = CreateSyntheticAssemblyWithVersion(version, referencedAssemblies);
            // Clear all types to avoid resolution errors when writing
            assembly.MainModule.Types.Clear();
            assembly.Write(filePath);
        }
    }
}
