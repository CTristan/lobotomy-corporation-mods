// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using Mono.Cecil;
using Xunit;
using Xunit.Abstractions;
using static RetargetHarmony.RetargetHarmony;

namespace RetargetHarmony.Test
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

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after patching");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should be retargeted to 0Harmony109");
        }

        [Fact]
        public void Patch_RetargetsHarmonyReference_InLobotomyBaseModLib()
        {
            using AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("LobotomyBaseModLib.dll"));

            Patch(assemblyDefinition);

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);

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
            using AssemblyDefinition assemblyDefinition = CreateSyntheticAssembly("0Harmony109");

            Patch(assemblyDefinition);
            Patch(assemblyDefinition);

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should still be exactly one Harmony reference");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the reference should remain 0Harmony109");
        }

        [Fact]
        public void Patch_RemovesDuplicateHarmonyReferences()
        {
            using AssemblyDefinition assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony", "0Harmony");

            Patch(assemblyDefinition);

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after deduplication");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be retargeted to 0Harmony109");
        }

        [Fact]
        public void Patch_ConsolidatesMixedHarmonyReferences()
        {
            using AssemblyDefinition assemblyDefinition = CreateSyntheticAssembly("0Harmony", "0Harmony109", "0Harmony");

            Patch(assemblyDefinition);

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);

            _ = harmonyRefs.Should().HaveCount(1, "there should be exactly one Harmony reference after consolidation");
            _ = harmonyRefs[0].Name.Should().Be("0Harmony109", "the remaining reference should be 0Harmony109");
        }

        [Fact]
        public void Patch_DoesNothing_WhenNoHarmonyReference()
        {
            using AssemblyDefinition assemblyDefinition = CreateSyntheticAssembly("mscorlib");

            Exception exception = Record.Exception(() => Patch(assemblyDefinition));

            _ = exception.Should().BeNull("patching an assembly without Harmony reference should not throw");

            List<AssemblyNameReference> harmonyRefs = GetHarmonyReferences(assemblyDefinition);
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
            MethodInfo? finishMethod = typeof(RetargetHarmony).GetMethod("Finish", BindingFlags.Public | BindingFlags.Static);

            _ = finishMethod.Should().NotBeNull("Finish() method should exist for BepInEx preloader lifecycle");
            _ = finishMethod.GetParameters().Should().BeEmpty("Finish() should take no parameters");
        }

        [Fact]
        public void BaseModsPath_ComputesCorrectPath()
        {
            // Verify BaseModsPath computes a valid path structure
            string baseModsPath = BaseModsPath;

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
        public void TargetDLLs_IncludesBaseModsDlls_WhenPatchBaseModsEnabled()
        {
            // Set override to true to enable BaseMods patching
            PatchBaseModsOverride = true;

            try
            {
                // Note: BaseMods directory may not exist in test environment
                // The property should handle this gracefully
                List<string> targetDlls = [.. TargetDLLs];

                // Should contain Managed DLLs
                _ = targetDlls.Should().Contain("Assembly-CSharp.dll");
                _ = targetDlls.Should().Contain("LobotomyBaseModLib.dll");

                // If BaseMods directory exists, should also contain those DLLs
                string baseModsPath = BaseModsPath;
                if (Directory.Exists(baseModsPath))
                {
                    string[] baseModsDlls = Directory.GetFiles(baseModsPath, "*.dll", SearchOption.TopDirectoryOnly);
                    foreach (string dll in baseModsDlls)
                    {
                        string dllName = Path.GetFileName(dll);
                        _ = targetDlls.Should().Contain(dllName, $"BaseMods DLL {dllName} should be included when PatchBaseMods is enabled");
                    }
                }
            }
            finally
            {
                // Clean up
                PatchBaseModsOverride = false;
            }
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
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(GetManagedAssemblyPath("Assembly-CSharp.dll"));
            assembly.MainModule.AssemblyReferences.Clear();

            foreach (string refName in referencedAssemblies)
            {
                AssemblyNameReference reference = new(refName, new Version(1, 0, 0, 0));
                assembly.MainModule.AssemblyReferences.Add(reference);
            }

            return assembly;
        }
    }
}
