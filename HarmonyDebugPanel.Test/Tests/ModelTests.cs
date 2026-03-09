// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using HarmonyDebugPanel.Models;
using Xunit;

namespace HarmonyDebugPanel.Test.Tests
{
    public sealed class ModelTests
    {
        [Fact]
        public void ModInfo_Constructor_AssignsValues()
        {
            ModInfo model = new("Test", "1.0.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony2, "TestAssembly", "guid.test");

            _ = model.Name.Should().Be("Test");
            _ = model.Version.Should().Be("1.0.0");
            _ = model.Source.Should().Be(ModSource.BepInExPlugin);
            _ = model.HarmonyVersion.Should().Be(HarmonyVersion.Harmony2);
            _ = model.AssemblyName.Should().Be("TestAssembly");
            _ = model.Identifier.Should().Be("guid.test");
        }

        [Fact]
        public void PatchInfo_Constructor_AssignsValues()
        {
            PatchInfo model = new("TargetType", "TargetMethod", PatchType.Postfix, "owner", "PatchType.Method", "PatchAssembly");

            _ = model.TargetType.Should().Be("TargetType");
            _ = model.TargetMethod.Should().Be("TargetMethod");
            _ = model.PatchType.Should().Be(PatchType.Postfix);
            _ = model.Owner.Should().Be("owner");
            _ = model.PatchMethod.Should().Be("PatchType.Method");
            _ = model.PatchAssemblyName.Should().Be("PatchAssembly");
        }

        [Fact]
        public void AssemblyInfo_Constructor_AssignsValues()
        {
            AssemblyInfo model = new("0Harmony", "2.7.0", "path/to/0Harmony.dll", true);

            _ = model.Name.Should().Be("0Harmony");
            _ = model.Version.Should().Be("2.7.0");
            _ = model.Location.Should().Be("path/to/0Harmony.dll");
            _ = model.IsHarmonyRelated.Should().BeTrue();
        }

        [Fact]
        public void DiagnosticReport_Defaults_AreInitialized()
        {
            DiagnosticReport report = new();

            _ = report.Mods.Should().NotBeNull();
            _ = report.Patches.Should().NotBeNull();
            _ = report.Assemblies.Should().NotBeNull();
            _ = report.Warnings.Should().NotBeNull();
            _ = report.RetargetHarmonyStatus.Should().NotBeNull();
            _ = report.CollectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}
