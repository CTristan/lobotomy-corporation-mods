// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test
{
    public sealed class BaseModCollectorTests
    {
        [Fact]
        public void Collect_ReturnsHarmony1Mod_WhenPatchAssemblyReferencesHarmony109()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "ModAssembly.PatchClass.PatchMethod",
                    "ModAssembly",
                    "1.0.0",
                    [new("0Harmony109")]),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().HaveCount(1);
            _ = mods[0].Name.Should().Be("ModAssembly");
            _ = mods[0].Version.Should().Be("1.0.0");
            _ = mods[0].Source.Should().Be(ModSource.Lmm);
            _ = mods[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony1);
            _ = mods[0].AssemblyName.Should().Be("ModAssembly");
            _ = mods[0].HasActivePatches.Should().BeTrue();
            _ = mods[0].ActivePatchCount.Should().Be(1);
        }

        [Fact]
        public void Collect_DoesNotIncludeHarmony2Mod_WhenPatchAssemblyReferencesHarmony2()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "Harmony2Mod.PatchClass.PatchMethod",
                    "Harmony2Mod",
                    "1.0.0",
                    [new("0Harmony")]),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().BeEmpty();
        }

        [Fact]
        public void Collect_CombinesPatchesFromSameAssembly_IntoSingleModWithCorrectCount()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType1",
                    "Method1",
                    PatchType.Prefix,
                    "owner1",
                    "ModAssembly.PatchClass.PatchMethod1",
                    "ModAssembly",
                    "1.0.0",
                    [new("0Harmony109")]),
                new(
                    "TargetType2",
                    "Method2",
                    PatchType.Postfix,
                    "owner2",
                    "ModAssembly.PatchClass.PatchMethod2",
                    "ModAssembly",
                    "1.0.0",
                    [new("0Harmony109")]),
                new(
                    "TargetType3",
                    "Method3",
                    PatchType.Transpiler,
                    "owner3",
                    "ModAssembly.PatchClass.PatchMethod3",
                    "ModAssembly",
                    "1.0.0",
                    [new("0Harmony109")]),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().HaveCount(1);
            _ = mods[0].Name.Should().Be("ModAssembly");
            _ = mods[0].ActivePatchCount.Should().Be(3);
        }

        [Fact]
        public void Collect_ReturnsEmpty_WhenNoPatches()
        {
            StubPatchInspectionSource patchSource = new([]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().BeEmpty();
        }

        [Fact]
        public void Collect_ExcludesFrameworkAssemblies_FromResults()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "mscorlib.PatchClass.PatchMethod",
                    "mscorlib",
                    "2.0.0.0",
                    []),
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "System.PatchClass.PatchMethod",
                    "System",
                    "2.0.0.0",
                    []),
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "UnityEngine.PatchClass.PatchMethod",
                    "UnityEngine",
                    "1.0.0.0",
                    []),
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "0Harmony109.PatchClass.PatchMethod",
                    "0Harmony109",
                    "1.2.0.1",
                    []),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().BeEmpty();
        }

        [Fact]
        public void Collect_ReturnsMultipleHarmony1Mods_WhenPatchesFromDifferentAssemblies()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType1",
                    "Method1",
                    PatchType.Prefix,
                    "owner1",
                    "Mod1.PatchClass.PatchMethod",
                    "Mod1",
                    "1.0.0",
                    [new("0Harmony109")]),
                new(
                    "TargetType2",
                    "Method2",
                    PatchType.Postfix,
                    "owner2",
                    "Mod2.PatchClass.PatchMethod",
                    "Mod2",
                    "2.0.0",
                    [new("0Harmony109")]),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().HaveCount(2);
            _ = mods.Should().ContainSingle(m => m.Name == "Mod1" && m.Version == "1.0.0");
            _ = mods.Should().ContainSingle(m => m.Name == "Mod2" && m.Version == "2.0.0");
        }

        [Fact]
        public void Collect_SkipsPatchesWithNullAssemblyName()
        {
            StubPatchInspectionSource patchSource = new(
            [
                new(
                    "TargetType",
                    "Method",
                    PatchType.Prefix,
                    "owner",
                    "ModAssembly.PatchClass.PatchMethod",
                    string.Empty,
                    "1.0.0",
                    [new("0Harmony109")]),
            ]);

            BaseModCollector collector = new(patchSource, new HarmonyVersionClassifier());

            IList<ModInfo> mods = collector.Collect();

            _ = mods.Should().BeEmpty();
        }

        private sealed class StubPatchInspectionSource(IEnumerable<PatchInspectionInfo> patches) : IPatchInspectionSource
        {
            private readonly IEnumerable<PatchInspectionInfo> _patches = patches;

            public IEnumerable<PatchInspectionInfo> GetPatches()
            {
                return _patches;
            }
        }
    }
}
