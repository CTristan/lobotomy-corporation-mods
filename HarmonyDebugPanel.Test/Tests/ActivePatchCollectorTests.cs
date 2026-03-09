// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class ActivePatchCollectorTests
    {
        [Fact]
        public void Collect_MapsPatchInspectionInfoToPatchInfo()
        {
            ActivePatchCollector collector = new(new StubPatchInspectionSource(
            [
                new("TargetType", "TargetMethod", PatchType.Prefix, "owner1", "PatchClass.Prefix", "Assembly1", "1.0.0", []),
                null,
                new("TargetType", "TargetMethod", PatchType.Postfix, "owner2", "PatchClass.Postfix", "Assembly2", "2.0.0", []),
            ]));

            IList<PatchInfo> patches = collector.Collect();

            _ = patches.Should().HaveCount(2);
            _ = patches.Should().ContainSingle(p => p.PatchType == PatchType.Prefix && p.Owner == "owner1" && p.PatchAssemblyName == "Assembly1");
            _ = patches.Should().ContainSingle(p => p.PatchType == PatchType.Postfix && p.Owner == "owner2" && p.PatchAssemblyName == "Assembly2");
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
