// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

#pragma warning disable CA1515 // Test classes must be public for xUnit

namespace HarmonyDebugPanel.Test;

public sealed class ActivePatchCollectorTests
{
    [Fact]
    public void Collect_MapsPatchInspectionInfoToPatchInfo()
    {
        var collector = new ActivePatchCollector(new StubPatchInspectionSource(new List<PatchInspectionInfo>
        {
            new("TargetType", "TargetMethod", PatchType.Prefix, "owner1", "PatchClass.Prefix", "Assembly1", "1.0.0", new List<AssemblyName>()),
            null,
            new("TargetType", "TargetMethod", PatchType.Postfix, "owner2", "PatchClass.Postfix", "Assembly2", "2.0.0", new List<AssemblyName>()),
        }));

        var patches = collector.Collect();

        patches.Should().HaveCount(2);
        patches.Should().ContainSingle(p => p.PatchType == PatchType.Prefix && p.Owner == "owner1" && p.PatchAssemblyName == "Assembly1");
        patches.Should().ContainSingle(p => p.PatchType == PatchType.Postfix && p.Owner == "owner2" && p.PatchAssemblyName == "Assembly2");
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
