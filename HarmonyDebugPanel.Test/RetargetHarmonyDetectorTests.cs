// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using HarmonyDebugPanel.Implementations.Collectors;
using HarmonyDebugPanel.Interfaces;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class RetargetHarmonyDetectorTests
{
    [Fact]
    public void Collect_ReturnsNotDetected_WhenRetargetHarmonyAssemblyMissing()
    {
        var detector = new RetargetHarmonyDetector(new StubAssemblySource(new List<AssemblyInspectionInfo>()));

        var status = detector.Collect();

        status.IsDetected.Should().BeFalse();
        status.Message.Should().Be("Not detected");
    }

    [Fact]
    public void Collect_ReturnsDetectedWithoutPatchedTargets_WhenHarmony109ReferencesMissing()
    {
        var detector = new RetargetHarmonyDetector(new StubAssemblySource(new List<AssemblyInspectionInfo>
        {
            new("RetargetHarmony", "1.0.0", "RetargetHarmony.dll", new List<AssemblyName>()),
            new("Assembly-CSharp", "1.0.0", "Assembly-CSharp.dll", new List<AssemblyName> { new("0Harmony") }),
            new("LobotomyBaseModLib", "1.0.0", "LobotomyBaseModLib.dll", new List<AssemblyName> { new("0Harmony") }),
        }));

        var status = detector.Collect();

        status.IsDetected.Should().BeTrue();
        status.AssemblyCSharpRetargeted.Should().BeFalse();
        status.LobotomyBaseModLibRetargeted.Should().BeFalse();
        status.Message.Should().Be("Detected (target assemblies not patched yet)");
    }

    [Fact]
    public void Collect_ReturnsDetectedWithPatchedTargets_WhenHarmony109ReferencesExist()
    {
        var detector = new RetargetHarmonyDetector(new StubAssemblySource(new List<AssemblyInspectionInfo>
        {
            new("RetargetHarmony", "1.0.0", "RetargetHarmony.dll", new List<AssemblyName>()),
            new("Assembly-CSharp", "1.0.0", "Assembly-CSharp.dll", new List<AssemblyName> { new("0Harmony109") }),
            new("LobotomyBaseModLib", "1.0.0", "LobotomyBaseModLib.dll", new List<AssemblyName> { new("0Harmony109") }),
        }));

        var status = detector.Collect();

        status.IsDetected.Should().BeTrue();
        status.AssemblyCSharpRetargeted.Should().BeTrue();
        status.LobotomyBaseModLibRetargeted.Should().BeTrue();
        status.Message.Should().Contain("Assembly-CSharp");
        status.Message.Should().Contain("LobotomyBaseModLib");
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
