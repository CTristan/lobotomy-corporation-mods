// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ActivePatchCollectorTests
    {
        private readonly Mock<IPatchInspectionSource> _mockSource;

        public ActivePatchCollectorTests()
        {
            _mockSource = new Mock<IPatchInspectionSource>();
            _mockSource.Setup(s => s.GetPatches()).Returns([]);
        }

        [Fact]
        public void Constructor_throws_when_patchInspectionSource_is_null()
        {
            Action act = () => _ = new ActivePatchCollector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patchInspectionSource");
        }

        [Fact]
        public void Collect_returns_empty_list_when_no_patches_exist()
        {
            var collector = new ActivePatchCollector(_mockSource.Object);

            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_maps_patch_inspection_info_to_patch_info()
        {
            var inspection = new PatchInspectionInfo(
                "GameClass", "GameMethod", PatchType.Postfix, "owner",
                "PatchMethod", "MyMod", "1.0", []);
            _mockSource.Setup(s => s.GetPatches()).Returns([inspection]);

            var collector = new ActivePatchCollector(_mockSource.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].TargetType.Should().Be("GameClass");
            result[0].TargetMethod.Should().Be("GameMethod");
            result[0].PatchType.Should().Be(PatchType.Postfix);
            result[0].Owner.Should().Be("owner");
            result[0].PatchMethod.Should().Be("PatchMethod");
            result[0].PatchAssemblyName.Should().Be("MyMod");
        }

        [Fact]
        public void Collect_skips_null_patches()
        {
            var inspection = new PatchInspectionInfo(
                "GameClass", "GameMethod", PatchType.Prefix, "owner",
                "PatchMethod", "MyMod", "1.0", []);
            _mockSource.Setup(s => s.GetPatches()).Returns([null, inspection]);

            var collector = new ActivePatchCollector(_mockSource.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
        }
    }
}
