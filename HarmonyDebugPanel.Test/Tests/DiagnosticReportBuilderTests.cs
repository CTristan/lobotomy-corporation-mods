// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class DiagnosticReportBuilderTests
    {
        [Fact]
        public void BuildReport_AggregatesCollectorData()
        {
            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>([new("Plugin", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id")]),
                new StubListCollector<ModInfo>([new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseModAssembly", string.Empty)]),
                new StubListCollector<PatchInfo>([new("Type", "Method", PatchType.Prefix, "owner", "patch", "PatchAssembly")]),
                new StubListCollector<AssemblyInfo>([new("Assembly", "1.0.0", "path", false)]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus(true, true, false, "Detected")),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            _ = report.Mods.Should().HaveCount(2);
            _ = report.Patches.Should().HaveCount(1);
            _ = report.Assemblies.Should().HaveCount(1);
            _ = report.RetargetHarmonyStatus.IsDetected.Should().BeTrue();
            _ = report.Warnings.Should().BeEmpty();
        }

        [Fact]
        public void BuildReport_ContinuesWhenCollectorThrows_AndAddsWarning()
        {
            DiagnosticReportBuilder builder = new(
                new ThrowingListCollector<ModInfo>("plugins failed"),
                new StubListCollector<ModInfo>([]),
                new ThrowingListCollector<PatchInfo>("patches failed"),
                new StubListCollector<AssemblyInfo>([]),
                new ThrowingValueCollector<RetargetHarmonyStatus>("retarget failed"),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            _ = report.Mods.Should().BeEmpty();
            _ = report.Patches.Should().BeEmpty();
            _ = report.Assemblies.Should().BeEmpty();
            _ = report.RetargetHarmonyStatus.Should().NotBeNull();
            _ = report.Warnings.Should().Contain(w => w.Contains("plugins failed", StringComparison.Ordinal));
            _ = report.Warnings.Should().Contain(w => w.Contains("patches failed", StringComparison.Ordinal));
            _ = report.Warnings.Should().Contain(w => w.Contains("retarget failed", StringComparison.Ordinal));
        }

        [Fact]
        public void BuildReport_CorrelatesPatchesWithModsWithMatchingAssemblyNames()
        {
            List<ModInfo> mods =
            [
                new("PluginMod", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id"),
                new("BaseMod1", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseMod1Assembly", string.Empty),
                new("BaseMod2", "2.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseMod2Assembly", string.Empty),
            ];

            List<PatchInfo> patches =
            [
                new("TargetType", "Method1", PatchType.Prefix, "owner1", "PluginAssembly.Namespace.PatchMethod", "PluginAssembly"),
                new("TargetType", "Method2", PatchType.Postfix, "owner2", "BaseMod1Assembly.PatchClass.PatchMethod", "BaseMod1Assembly"),
                new("TargetType", "Method3", PatchType.Transpiler, "owner3", "BaseMod1Assembly.PatchClass.AnotherPatchMethod", "BaseMod1Assembly"),
            ];

            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>(mods),
                new StubListCollector<ModInfo>([]),
                new StubListCollector<PatchInfo>(patches),
                new StubListCollector<AssemblyInfo>([]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            _ = report.Mods.Should().HaveCount(3);

            ModInfo pluginMod = report.Mods.Should().ContainSingle(m => m.Name == "PluginMod").Subject;
            _ = pluginMod.HasActivePatches.Should().BeTrue();
            _ = pluginMod.ActivePatchCount.Should().Be(1);

            ModInfo baseMod1 = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod1").Subject;
            _ = baseMod1.HasActivePatches.Should().BeTrue();
            _ = baseMod1.ActivePatchCount.Should().Be(2);

            ModInfo baseMod2 = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod2").Subject;
            _ = baseMod2.HasActivePatches.Should().BeFalse();
            _ = baseMod2.ActivePatchCount.Should().Be(0);
        }

        [Fact]
        public void BuildReport_CorrelatesPatchesByAssemblyName_DirectMatch()
        {
            List<ModInfo> mods =
            [
                new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "MyMod", string.Empty),
            ];

            List<PatchInfo> patches =
            [
                new("Target", "Method", PatchType.Prefix, "owner", "MyMod.Patches.PatchClass.PatchMethod", "MyMod"),
                new("Target", "Method", PatchType.Postfix, "owner", "MyMod.Harmony.PatchClass.AnotherPatch", "MyMod"),
            ];

            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>([]),
                new StubListCollector<ModInfo>(mods),
                new StubListCollector<PatchInfo>(patches),
                new StubListCollector<AssemblyInfo>([]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            ModInfo mod = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod").Subject;
            _ = mod.HasActivePatches.Should().BeTrue();
            _ = mod.ActivePatchCount.Should().Be(2);
        }

        [Fact]
        public void BuildReport_DoesNotCorrelatePatchesWithDifferentAssemblyName()
        {
            List<ModInfo> mods =
            [
                new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "MyMod", string.Empty),
            ];

            List<PatchInfo> patches =
            [
                new("Target", "Method", PatchType.Prefix, "owner", "OtherMod.PatchMethod", "OtherMod"),
            ];

            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>([]),
                new StubListCollector<ModInfo>(mods),
                new StubListCollector<PatchInfo>(patches),
                new StubListCollector<AssemblyInfo>([]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            ModInfo mod = report.Mods.Should().ContainSingle(m => m.Name == "BaseMod").Subject;
            _ = mod.HasActivePatches.Should().BeFalse();
            _ = mod.ActivePatchCount.Should().Be(0);
        }

        [Fact]
        public void BuildReport_HandlesEmptyPatchList()
        {
            List<ModInfo> mods =
            [
                new("NoPatchesMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "NoPatchesMod", string.Empty),
            ];

            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>([]),
                new StubListCollector<ModInfo>(mods),
                new StubListCollector<PatchInfo>([]),
                new StubListCollector<AssemblyInfo>([]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
                new StubExpectedPatchSource());

            DiagnosticReport report = builder.BuildReport();

            ModInfo mod = report.Mods.Should().ContainSingle(m => m.Name == "NoPatchesMod").Subject;
            _ = mod.HasActivePatches.Should().BeFalse();
            _ = mod.ActivePatchCount.Should().Be(0);
        }

        [Fact]
        public void BuildReport_HandlesNullModListOrPatchList()
        {
            DiagnosticReportBuilder builder = new(
                new StubListCollector<ModInfo>(null),
                new StubListCollector<ModInfo>(null),
                new StubListCollector<PatchInfo>(null),
                new StubListCollector<AssemblyInfo>([]),
                new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus()),
                new StubExpectedPatchSource());

            // Should not throw even with null collectors
            DiagnosticReport report = builder.BuildReport();

            // CollectSafe returns empty lists on null, not null
            _ = report.Mods.Should().BeEmpty();
            _ = report.Patches.Should().BeEmpty();
        }

        private sealed class StubListCollector<T>(IList<T> values) : IInfoCollector<IList<T>>
        {
            private readonly IList<T> _values = values;

            public IList<T> Collect()
            {
                return _values;
            }
        }

        private sealed class StubValueCollector<T>(T value) : IInfoCollector<T>
        {
            private readonly T _value = value;

            public T Collect()
            {
                return _value;
            }
        }

        private sealed class ThrowingListCollector<T>(string message) : IInfoCollector<IList<T>>
        {
            private readonly string _message = message;

            public IList<T> Collect()
            {
                throw new InvalidOperationException(_message);
            }
        }

        private sealed class ThrowingValueCollector<T>(string message) : IInfoCollector<T>
            where T : new()
        {
            private readonly string _message = message;

            public T Collect()
            {
                throw new InvalidOperationException(_message);
            }
        }

        private sealed class StubExpectedPatchSource : IExpectedPatchSource
        {
            public IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo)
            {
                return
                [
                    new ExpectedPatchInfo("Mod1", "TargetType1", "TargetMethod1", "PatchMethod1", PatchType.Prefix),
                    new ExpectedPatchInfo("Mod1", "TargetType2", "TargetMethod2", "PatchMethod2", PatchType.Postfix),
                ];
            }
        }
    }
}
