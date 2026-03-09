// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class BaseModCollectorTests
{
    [Fact]
    public void Collect_ReturnsHarmony1Mod_WhenPatchAssemblyReferencesHarmony109()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "ModAssembly.PatchClass.PatchMethod",
                "ModAssembly",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().HaveCount(1);
        mods[0].Name.Should().Be("ModAssembly");
        mods[0].Version.Should().Be("1.0.0");
        mods[0].Source.Should().Be(ModSource.Lmm);
        mods[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony1);
        mods[0].AssemblyName.Should().Be("ModAssembly");
        mods[0].HasActivePatches.Should().BeTrue();
        mods[0].ActivePatchCount.Should().Be(1);
    }

    [Fact]
    public void Collect_DoesNotIncludeHarmony2Mod_WhenPatchAssemblyReferencesHarmony2()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "Harmony2Mod.PatchClass.PatchMethod",
                "Harmony2Mod",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony") }),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().BeEmpty();
    }

    [Fact]
    public void Collect_CombinesPatchesFromSameAssembly_IntoSingleModWithCorrectCount()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType1",
                "Method1",
                PatchType.Prefix,
                "owner1",
                "ModAssembly.PatchClass.PatchMethod1",
                "ModAssembly",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
            new(
                "TargetType2",
                "Method2",
                PatchType.Postfix,
                "owner2",
                "ModAssembly.PatchClass.PatchMethod2",
                "ModAssembly",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
            new(
                "TargetType3",
                "Method3",
                PatchType.Transpiler,
                "owner3",
                "ModAssembly.PatchClass.PatchMethod3",
                "ModAssembly",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().HaveCount(1);
        mods[0].Name.Should().Be("ModAssembly");
        mods[0].ActivePatchCount.Should().Be(3);
    }

    [Fact]
    public void Collect_ReturnsEmpty_WhenNoPatches()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>());

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().BeEmpty();
    }

    [Fact]
    public void Collect_ExcludesFrameworkAssemblies_FromResults()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "mscorlib.PatchClass.PatchMethod",
                "mscorlib",
                "2.0.0.0",
                new List<AssemblyName>()),
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "System.PatchClass.PatchMethod",
                "System",
                "2.0.0.0",
                new List<AssemblyName>()),
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "UnityEngine.PatchClass.PatchMethod",
                "UnityEngine",
                "1.0.0.0",
                new List<AssemblyName>()),
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "0Harmony109.PatchClass.PatchMethod",
                "0Harmony109",
                "1.2.0.1",
                new List<AssemblyName>()),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().BeEmpty();
    }

    [Fact]
    public void Collect_ReturnsMultipleHarmony1Mods_WhenPatchesFromDifferentAssemblies()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType1",
                "Method1",
                PatchType.Prefix,
                "owner1",
                "Mod1.PatchClass.PatchMethod",
                "Mod1",
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
            new(
                "TargetType2",
                "Method2",
                PatchType.Postfix,
                "owner2",
                "Mod2.PatchClass.PatchMethod",
                "Mod2",
                "2.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().HaveCount(2);
        mods.Should().ContainSingle(m => m.Name == "Mod1" && m.Version == "1.0.0");
        mods.Should().ContainSingle(m => m.Name == "Mod2" && m.Version == "2.0.0");
    }

    [Fact]
    public void Collect_SkipsPatchesWithNullAssemblyName()
    {
        var patchSource = new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new(
                "TargetType",
                "Method",
                PatchType.Prefix,
                "owner",
                "ModAssembly.PatchClass.PatchMethod",
                string.Empty,
                "1.0.0",
                new List<AssemblyName> { new("0Harmony109") }),
        });

        var collector = new BaseModCollector(patchSource, new HarmonyVersionClassifier());

        var mods = collector.Collect();

        mods.Should().BeEmpty();
    }

    private sealed class StubPatchInspectionSource : IPatchInspectionSource
    {
        private readonly IEnumerable<PatchInspectionInfo> _patches;

        public StubPatchInspectionSource(IEnumerable<PatchInspectionInfo> patches)
        {
            _patches = patches;
        }

        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            return _patches;
        }
    }
}
