// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test;

public sealed class ModelTests
{
    [Fact]
    public void ModInfo_Constructor_AssignsValues()
    {
        var model = new ModInfo("Test", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "TestAssembly", "guid.test");

        model.Name.Should().Be("Test");
        model.Version.Should().Be("1.0.0");
        model.Source.Should().Be(ModSource.BepInExPlugin);
        model.HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
        model.AssemblyName.Should().Be("TestAssembly");
        model.Identifier.Should().Be("guid.test");
    }

    [Fact]
    public void PatchInfo_Constructor_AssignsValues()
    {
        var model = new PatchInfo("TargetType", "TargetMethod", PatchType.Postfix, "owner", "PatchType.Method", "PatchAssembly");

        model.TargetType.Should().Be("TargetType");
        model.TargetMethod.Should().Be("TargetMethod");
        model.PatchType.Should().Be(PatchType.Postfix);
        model.Owner.Should().Be("owner");
        model.PatchMethod.Should().Be("PatchType.Method");
        model.PatchAssemblyName.Should().Be("PatchAssembly");
    }

    [Fact]
    public void AssemblyInfo_Constructor_AssignsValues()
    {
        var model = new AssemblyInfo("0Harmony", "2.7.0", "path/to/0Harmony.dll", true);

        model.Name.Should().Be("0Harmony");
        model.Version.Should().Be("2.7.0");
        model.Location.Should().Be("path/to/0Harmony.dll");
        model.IsHarmonyRelated.Should().BeTrue();
    }

    [Fact]
    public void DiagnosticReport_Defaults_AreInitialized()
    {
        var report = new DiagnosticReport();

        report.Mods.Should().NotBeNull();
        report.Patches.Should().NotBeNull();
        report.Assemblies.Should().NotBeNull();
        report.Warnings.Should().NotBeNull();
        report.RetargetHarmonyStatus.Should().NotBeNull();
        report.CollectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
