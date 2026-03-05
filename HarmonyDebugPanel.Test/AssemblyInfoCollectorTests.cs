// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class AssemblyInfoCollectorTests
{
    [Fact]
    public void Collect_FlagsHarmonyAssemblies()
    {
        var collector = new AssemblyInfoCollector(new StubAssemblySource(new List<AssemblyInspectionInfo>
        {
            new("0Harmony", "2.7.0", "0Harmony.dll", new List<AssemblyName>()),
            null,
            new("SomeOtherAssembly", "1.0.0", "SomeOtherAssembly.dll", new List<AssemblyName>()),
        }));

        var results = collector.Collect();

        results.Should().HaveCount(2);
        results.Should().ContainSingle(a => a.Name == "0Harmony" && a.IsHarmonyRelated);
        results.Should().ContainSingle(a => a.Name == "SomeOtherAssembly" && !a.IsHarmonyRelated);
    }

    private sealed class StubAssemblySource : IAssemblyInspectionSource
    {
        private readonly IEnumerable<AssemblyInspectionInfo> _assemblies;

        public StubAssemblySource(IEnumerable<AssemblyInspectionInfo> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<AssemblyInspectionInfo> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
