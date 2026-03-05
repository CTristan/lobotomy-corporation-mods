// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class DiagnosticReportBuilderTests
{
    [Fact]
    public void BuildReport_AggregatesCollectorData()
    {
        var builder = new DiagnosticReportBuilder(
            new StubListCollector<ModInfo>(new List<ModInfo> { new("Plugin", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "PluginAssembly", "plugin.id") }),
            new StubListCollector<ModInfo>(new List<ModInfo> { new("BaseMod", "1.0.0", ModSource.Lmm, HarmonyVersion.Harmony1, "BaseModAssembly", string.Empty) }),
            new StubListCollector<PatchInfo>(new List<PatchInfo> { new("Type", "Method", PatchType.Prefix, "owner", "patch") }),
            new StubListCollector<AssemblyInfo>(new List<AssemblyInfo> { new("Assembly", "1.0.0", "path", false) }),
            new StubValueCollector<RetargetHarmonyStatus>(new RetargetHarmonyStatus(true, true, false, "Detected")));

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
            new ThrowingValueCollector<RetargetHarmonyStatus>("retarget failed"));

        var report = builder.BuildReport();

        report.Mods.Should().BeEmpty();
        report.Patches.Should().BeEmpty();
        report.Assemblies.Should().BeEmpty();
        report.RetargetHarmonyStatus.Should().NotBeNull();
        report.Warnings.Should().Contain(w => w.Contains("plugins failed", StringComparison.Ordinal));
        report.Warnings.Should().Contain(w => w.Contains("patches failed", StringComparison.Ordinal));
        report.Warnings.Should().Contain(w => w.Contains("retarget failed", StringComparison.Ordinal));
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
}
