// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class AssemblyInfoCollectorTests
    {
        private readonly Mock<IAssemblyInspectionSource> _mockSource;

        public AssemblyInfoCollectorTests()
        {
            _mockSource = new Mock<IAssemblyInspectionSource>();
            _mockSource.Setup(s => s.GetAssemblies()).Returns([]);
        }

        [Fact]
        public void Constructor_throws_when_assemblySource_is_null()
        {
            Action act = () => _ = new AssemblyInfoCollector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assemblySource");
        }

        [Fact]
        public void Collect_returns_empty_list_when_no_assemblies_exist()
        {
            var collector = new AssemblyInfoCollector(_mockSource.Object);

            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_maps_assembly_inspection_info_to_assembly_info()
        {
            var assembly = new AssemblyInspectionInfo("TestDll", "2.0.0", "/path/test.dll", []);
            _mockSource.Setup(s => s.GetAssemblies()).Returns([assembly]);

            var collector = new AssemblyInfoCollector(_mockSource.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("TestDll");
            result[0].Version.Should().Be("2.0.0");
            result[0].Location.Should().Be("/path/test.dll");
            result[0].IsHarmonyRelated.Should().BeFalse();
        }

        [Fact]
        public void Collect_marks_assembly_as_harmony_related_when_name_contains_harmony()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                new("0Harmony", "2.0.0", "/path/harmony.dll", []),
                new("MyHarmonyLib", "1.0.0", "/path/lib.dll", []),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var collector = new AssemblyInfoCollector(_mockSource.Object);
            var result = collector.Collect();

            result.Should().HaveCount(2);
            result[0].IsHarmonyRelated.Should().BeTrue();
            result[1].IsHarmonyRelated.Should().BeTrue();
        }

        [Fact]
        public void Collect_skips_null_assemblies()
        {
            var assembly = new AssemblyInspectionInfo("TestDll", "1.0.0", "/path/test.dll", []);
            _mockSource.Setup(s => s.GetAssemblies()).Returns([null, assembly]);

            var collector = new AssemblyInfoCollector(_mockSource.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
        }
    }
}
