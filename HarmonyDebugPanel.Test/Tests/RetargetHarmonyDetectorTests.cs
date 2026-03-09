// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using AwesomeAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class RetargetHarmonyDetectorTests
    {
        [Fact]
        public void Collect_ReturnsNotDetected_WhenRetargetHarmonyAssemblyMissing()
        {
            RetargetHarmonyDetector detector = new(new StubAssemblySource([]));

            RetargetHarmonyStatus status = detector.Collect();

            _ = status.IsDetected.Should().BeFalse();
            _ = status.Message.Should().Be("Not detected");
        }

        [Fact]
        public void Collect_ReturnsDetectedWithoutPatchedTargets_WhenHarmony109ReferencesMissing()
        {
            RetargetHarmonyDetector detector = new(new StubAssemblySource(
            [
                new("RetargetHarmony", "1.0.0", "RetargetHarmony.dll", []),
                new("Assembly-CSharp", "1.0.0", "Assembly-CSharp.dll", [new("0Harmony")]),
                new("LobotomyBaseModLib", "1.0.0", "LobotomyBaseModLib.dll", [new("0Harmony")]),
            ]));

            RetargetHarmonyStatus status = detector.Collect();

            _ = status.IsDetected.Should().BeTrue();
            _ = status.AssemblyCSharpRetargeted.Should().BeFalse();
            _ = status.LobotomyBaseModLibRetargeted.Should().BeFalse();
            _ = status.Message.Should().Be("Detected (target assemblies not patched yet)");
        }

        [Fact]
        public void Collect_ReturnsDetectedWithPatchedTargets_WhenHarmony109ReferencesExist()
        {
            RetargetHarmonyDetector detector = new(new StubAssemblySource(
            [
                new("RetargetHarmony", "1.0.0", "RetargetHarmony.dll", []),
                new("Assembly-CSharp", "1.0.0", "Assembly-CSharp.dll", [new("0Harmony109")]),
                new("LobotomyBaseModLib", "1.0.0", "LobotomyBaseModLib.dll", [new("0Harmony109")]),
            ]));

            RetargetHarmonyStatus status = detector.Collect();

            _ = status.IsDetected.Should().BeTrue();
            _ = status.AssemblyCSharpRetargeted.Should().BeTrue();
            _ = status.LobotomyBaseModLibRetargeted.Should().BeTrue();
            _ = status.Message.Should().Contain("Assembly-CSharp");
            _ = status.Message.Should().Contain("LobotomyBaseModLib");
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
