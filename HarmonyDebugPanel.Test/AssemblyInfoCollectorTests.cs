// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test
{
    public sealed class AssemblyInfoCollectorTests
    {
        [Fact]
        public void Collect_FlagsHarmonyAssemblies()
        {
            AssemblyInfoCollector collector = new(new StubAssemblySource(
            [
                new("0Harmony", "2.7.0", "0Harmony.dll", []),
                null,
                new("SomeOtherAssembly", "1.0.0", "SomeOtherAssembly.dll", []),
            ]));

            IList<AssemblyInfo> results = collector.Collect();

            _ = results.Should().HaveCount(2);
            _ = results.Should().ContainSingle(a => a.Name == "0Harmony" && a.IsHarmonyRelated);
            _ = results.Should().ContainSingle(a => a.Name == "SomeOtherAssembly" && !a.IsHarmonyRelated);
        }

        private sealed class StubAssemblySource(IEnumerable<AssemblyInspectionInfo> assemblies) : IAssemblyInspectionSource
        {
            private readonly IEnumerable<AssemblyInspectionInfo> _assemblies = assemblies;

            public IEnumerable<AssemblyInspectionInfo> GetAssemblies()
            {
                return _assemblies;
            }
        }
    }
}
