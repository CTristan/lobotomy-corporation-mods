// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class BaseModCollectorTests
    {
        private readonly Mock<IPatchInspectionSource> _mockSource;
        private readonly Mock<IHarmonyVersionClassifier> _mockClassifier;

        public BaseModCollectorTests()
        {
            _mockSource = new Mock<IPatchInspectionSource>();
            _mockClassifier = new Mock<IHarmonyVersionClassifier>();
            _mockSource.Setup(s => s.GetPatches()).Returns([]);
        }

        private static PatchInspectionInfo CreatePatch(string assemblyName, IList<AssemblyName>? references = null)
        {
            return new PatchInspectionInfo(
                "TargetType", "TargetMethod", PatchType.Postfix, "owner",
                "PatchMethod", assemblyName, "1.0.0",
                references ?? []);
        }

        [Fact]
        public void Constructor_throws_when_patchInspectionSource_is_null()
        {
            Action act = () => _ = new BaseModCollector(null, _mockClassifier.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patchInspectionSource");
        }

        [Fact]
        public void Constructor_throws_when_harmonyVersionClassifier_is_null()
        {
            Action act = () => _ = new BaseModCollector(_mockSource.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("harmonyVersionClassifier");
        }

        [Fact]
        public void Collect_returns_empty_list_when_no_patches_exist()
        {
            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);

            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_mod_for_Harmony1_classified_assembly()
        {
            var patch = CreatePatch("MyMod");
            _mockSource.Setup(s => s.GetPatches()).Returns([patch]);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony1);

            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("MyMod");
            result[0].Source.Should().Be(ModSource.Lmm);
            result[0].HarmonyVersion.Should().Be(HarmonyVersion.Harmony1);
            result[0].ActivePatchCount.Should().Be(1);
        }

        [Fact]
        public void Collect_skips_assemblies_classified_as_Harmony2()
        {
            var patch = CreatePatch("MyH2Mod");
            _mockSource.Setup(s => s.GetPatches()).Returns([patch]);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony2);

            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_skips_framework_assemblies()
        {
            var patches = new List<PatchInspectionInfo>
            {
                CreatePatch("mscorlib"),
                CreatePatch("UnityEngine"),
                CreatePatch("System.Core"),
            };
            _mockSource.Setup(s => s.GetPatches()).Returns(patches);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony1);

            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Collect_groups_multiple_patches_from_same_assembly()
        {
            var patches = new List<PatchInspectionInfo>
            {
                CreatePatch("MyMod"),
                CreatePatch("MyMod"),
                CreatePatch("MyMod"),
            };
            _mockSource.Setup(s => s.GetPatches()).Returns(patches);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony1);

            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].ActivePatchCount.Should().Be(3);
        }

        [Fact]
        public void Collect_skips_null_and_empty_assembly_name_patches()
        {
            var validPatch = CreatePatch("MyMod");
            var emptyNamePatch = CreatePatch("");
            _mockSource.Setup(s => s.GetPatches()).Returns([null, emptyNamePatch, validPatch]);
            _mockClassifier.Setup(c => c.Classify(It.IsAny<IList<AssemblyName>>())).Returns(HarmonyVersion.Harmony1);

            var collector = new BaseModCollector(_mockSource.Object, _mockClassifier.Object);
            var result = collector.Collect();

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("MyMod");
        }
    }
}
