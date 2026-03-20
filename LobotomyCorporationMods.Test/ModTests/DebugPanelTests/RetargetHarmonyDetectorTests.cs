// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class RetargetHarmonyDetectorTests
    {
        private readonly Mock<IAssemblyInspectionSource> _mockSource;

        public RetargetHarmonyDetectorTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockSource = fixture.Freeze<Mock<IAssemblyInspectionSource>>();
            _mockSource.Setup(s => s.GetAssemblies()).Returns([]);
        }

        private static AssemblyInspectionInfo CreateAssembly(string name, params string[] referenceNames)
        {
            var references = new List<AssemblyName>();
            foreach (var refName in referenceNames)
            {
                references.Add(new AssemblyName(refName));
            }

            return new AssemblyInspectionInfo(name, "1.0.0", "/path/" + name + ".dll", references);
        }

        [Fact]
        public void Constructor_throws_when_assemblySource_is_null()
        {
            Action act = () => _ = new RetargetHarmonyDetector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assemblySource");
        }

        [Fact]
        public void Collect_returns_not_detected_when_RetargetHarmony_assembly_is_absent()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                CreateAssembly("Assembly-CSharp"),
                CreateAssembly("LobotomyBaseModLib"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeFalse();
            result.Message.Should().Be("Not detected");
        }

        [Fact]
        public void Collect_returns_detected_with_not_patched_yet_when_targets_lack_0Harmony109()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                CreateAssembly("RetargetHarmony"),
                CreateAssembly("Assembly-CSharp", "0Harmony"),
                CreateAssembly("LobotomyBaseModLib", "0Harmony"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeTrue();
            result.AssemblyCSharpRetargeted.Should().BeFalse();
            result.LobotomyBaseModLibRetargeted.Should().BeFalse();
            result.Message.Should().Contain("not patched yet");
        }

        [Fact]
        public void Collect_returns_detected_with_Assembly_CSharp_patched()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                CreateAssembly("RetargetHarmony"),
                CreateAssembly("Assembly-CSharp", "0Harmony109"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeTrue();
            result.AssemblyCSharpRetargeted.Should().BeTrue();
            result.Message.Should().Contain("Assembly-CSharp");
        }

        [Fact]
        public void Collect_returns_detected_with_LobotomyBaseModLib_patched()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                CreateAssembly("RetargetHarmony"),
                CreateAssembly("LobotomyBaseModLib", "0Harmony109"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeTrue();
            result.LobotomyBaseModLibRetargeted.Should().BeTrue();
            result.Message.Should().Contain("LobotomyBaseModLib");
        }

        [Fact]
        public void Collect_returns_detected_with_both_patched()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                CreateAssembly("RetargetHarmony"),
                CreateAssembly("Assembly-CSharp", "0Harmony109"),
                CreateAssembly("LobotomyBaseModLib", "0Harmony109"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeTrue();
            result.AssemblyCSharpRetargeted.Should().BeTrue();
            result.LobotomyBaseModLibRetargeted.Should().BeTrue();
            result.Message.Should().Contain("Assembly-CSharp").And.Contain("LobotomyBaseModLib");
        }

        [Fact]
        public void Collect_skips_null_assemblies()
        {
            var assemblies = new List<AssemblyInspectionInfo>
            {
                null!,
                CreateAssembly("RetargetHarmony"),
                null!,
                CreateAssembly("Assembly-CSharp", "0Harmony109"),
            };
            _mockSource.Setup(s => s.GetAssemblies()).Returns(assemblies);

            var detector = new RetargetHarmonyDetector(_mockSource.Object);
            var result = detector.Collect();

            result.IsDetected.Should().BeTrue();
            result.AssemblyCSharpRetargeted.Should().BeTrue();
        }
    }
}
