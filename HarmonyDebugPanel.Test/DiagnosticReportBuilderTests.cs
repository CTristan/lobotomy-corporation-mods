// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class DiagnosticReportBuilderTests
{
    [Fact]
    public void BuildReport_AggregatesCollectorData()
    {
        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(new List<ModInfo> { new("Plugin", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id") }),
            new StubListCollector<ModInfo>(new List<ModInfo> { new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseModAssembly", string.Empty) }),
            new StubListCollector<PatchInfo>(new List<PatchInfo> { new("Type", "Method", PatchType.Prefix, "owner", "patch", "PatchAssembly") }),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo> { new("Assembly", "1.0.0", "path", false) }),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus(true, true, false, "Detected")),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        report.Mods.Should().HaveCount(2);
        report.Patches.Should().HaveCount(1);
        report.Assemblies.Should().HaveCount(1);
        report.RetargetHarmonyStatus.IsDetected.Should().BeTrue();
        report.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void BuildReport_ContinuesWhenCollectorThrows_AndAddsWarning()
    {
        var builder = new DiagnosticReportBuilder(
            new ThrowingListCollector<ModInfo>("plugins failed"),
            new StubListCollector<ModInfo>(new List<ModInfo>()),
            new ThrowingListCollector<PatchInfo>("patches failed"),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new ThrowingValueCollector<RetargetHarmonyStatus>("retarget failed"),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        report.Mods.Should().BeEmpty();
        report.Patches.Should().BeEmpty();
        report.Assemblies.Should().BeEmpty();
        report.RetargetHarmonyStatus.Should().NotBeNull();
        report.Warnings.Should().Contain(w => w.Contains("plugins failed", StringComparison.Ordinal));
        report.Warnings.Should().Contain(w => w.Contains("patches failed", StringComparison.Ordinal));
        report.Warnings.Should().Contain(w => w.Contains("retarget failed", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildReport_CorrelatesPatchesWithModsWithMatchingAssemblyNames()
    {
        var mods = new List<ModInfo>
        {
            new("PluginMod", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id"),
            new("BaseMod1", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseMod1Assembly", string.Empty),
            new("BaseMod2", "2.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseMod2Assembly", string.Empty),
        };

        var patches = new List<PatchInfo>
        {
            new("TargetType", "Method1", PatchType.Prefix, "owner1", "PluginAssembly.Namespace.PatchMethod", "PluginAssembly"),
            new("TargetType", "Method2", PatchType.Postfix, "owner2", "BaseMod1Assembly.PatchClass.PatchMethod", "BaseMod1Assembly"),
            new("TargetType", "Method3", PatchType.Transpiler, "owner3", "BaseMod1Assembly.PatchClass.AnotherPatchMethod", "BaseMod1Assembly"),
        };

        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(mods),
            new StubListCollector<ModInfo>(new List<ModInfo>()),
            new StubListCollector<PatchInfo>(patches),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        report.Mods.Should().HaveCount(3);

        var pluginMod = report.Mods.Should().ContainSingle(m => m.Name == "PluginMod").Subject;
        pluginMod.HasActivePatches.Should().BeTrue();
        pluginMod.ActivePatchCount.Should().Be(1);

        var baseMod1 = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod1").Subject;
        baseMod1.HasActivePatches.Should().BeTrue();
        baseMod1.ActivePatchCount.Should().Be(2);

        var baseMod2 = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod2").Subject;
        baseMod2.HasActivePatches.Should().BeFalse();
        baseMod2.ActivePatchCount.Should().Be(0);
    }

    [Fact]
    public void BuildReport_CorrelatesPatchesByAssemblyName_DirectMatch()
    {
        var mods = new List<ModInfo>
        {
            new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "MyMod", string.Empty),
        };

        var patches = new List<PatchInfo>
        {
            new("Target", "Method", PatchType.Prefix, "owner", "MyMod.Patches.PatchClass.PatchMethod", "MyMod"),
            new("Target", "Method", PatchType.Postfix, "owner", "MyMod.Harmony.PatchClass.AnotherPatch", "MyMod"),
        };

        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(new List<ModInfo>()),
            new StubListCollector<ModInfo>(mods),
            new StubListCollector<PatchInfo>(patches),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        var mod = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod").Subject;
        mod.HasActivePatches.Should().BeTrue();
        mod.ActivePatchCount.Should().Be(2);
    }

    [Fact]
    public void BuildReport_DoesNotCorrelatePatchesWithDifferentAssemblyName()
    {
        var mods = new List<ModInfo>
        {
            new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "MyMod", string.Empty),
        };

        var patches = new List<PatchInfo>
        {
            new("Target", "Method", PatchType.Prefix, "owner", "OtherMod.PatchMethod", "OtherMod"),
        };

        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(new List<ModInfo>()),
            new StubListCollector<ModInfo>(mods),
            new StubListCollector<PatchInfo>(patches),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        var mod = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod").Subject;
        mod.HasActivePatches.Should().BeFalse();
        mod.ActivePatchCount.Should().Be(0);
    }

    [Fact]
    public void BuildReport_HandlesEmptyPatchList()
    {
        var mods = new List<ModInfo>
        {
            new("NoPatchesMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "NoPatchesMod", string.Empty),
        };

        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(new List<ModInfo>()),
            new StubListCollector<ModInfo>(mods),
            new StubListCollector<PatchInfo>(new List<PatchInfo>()),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
            new StubExpectedPatchSource());

        var report = builder.BuildReport();

        var mod = report.Mods.Should().ContainSingle(m => m.Name == "NoPatchesMod").Subject;
        mod.HasActivePatches.Should().BeFalse();
        mod.ActivePatchCount.Should().Be(0);
    }

    [Fact]
    public void BuildReport_HandlesNullModListOrPatchList()
    {
        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(null),
            new StubListCollector<ModInfo>(null),
            new StubListCollector<PatchInfo>(null),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo>()),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
            new StubExpectedPatchSource());

        // Should not throw even with null collectors
        var report = builder.BuildReport();

        // CollectSafe returns empty lists on null, not null
        report.Mods.Should().BeEmpty();
        report.Patches.Should().BeEmpty();
    }

    private sealed class StubListCollector<T> : IInfoCollector<IList<T>>
    {
        private readonly IList<T> _values;

        public StubListCollector(IList<T> values)
        {
            _values = values;
        }

        public IList<T> Collect()
        {
            return _values;
        }
    }

    private sealed class StubValueCollector<T> : IInfoCollector<T>
    {
        private readonly T _value;

        public StubValueCollector(T value)
        {
            _value = value;
        }

        public T Collect()
        {
            return _value;
        }
    }

    private sealed class ThrowingListCollector<T> : IInfoCollector<IList<T>>
    {
        private readonly string _message;

        public ThrowingListCollector(string message)
        {
            _message = message;
        }

        public IList<T> Collect()
        {
            throw new InvalidOperationException(_message);
        }
    }

    private sealed class ThrowingValueCollector<T> : IInfoCollector<T>
        where T : new()
    {
        private readonly string _message;

        public ThrowingValueCollector(string message)
        {
            _message = message;
        }

        public T Collect()
        {
            throw new InvalidOperationException(_message);
        }
    }

    private sealed class StubExpectedPatchSource : IExpectedPatchSource
    {
        public IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo)
        {
            return new List<ExpectedPatchInfo>
            {
                new ExpectedPatchInfo("Mod1", "TargetType1", "TargetMethod1", "PatchMethod1", PatchType.Prefix),
                new ExpectedPatchInfo("Mod1", "TargetType2", "TargetMethod2", "PatchMethod2", PatchType.Postfix),
            };
        }
    }
}
