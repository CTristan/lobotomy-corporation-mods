// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DiagnosticReportTests
    {
        private static DllIntegrityReport CreateDefaultDllIntegrityReport()
        {
            return new DllIntegrityReport(
                [], false, string.Empty, false, string.Empty,
                -1, false, 0, [], "No findings");
        }

        [Fact]
        public void Constructor_stores_all_properties()
        {
            var mods = new List<DetectedModInfo>();
            var patches = new List<PatchInfo>();
            var assemblies = new List<AssemblyInfo>();
            var patchComparison = new PatchComparisonResult([], 0, 0);
            var retargetStatus = new RetargetHarmonyStatus(true, true, false, "Detected");
            var envInfo = new EnvironmentInfo(true, true, false);
            var dllIntegrity = CreateDefaultDllIntegrityReport();
            var warnings = new List<string> { "test warning" };
            var debugInfo = new List<string> { "debug line" };
            var collectedAt = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

            var report = new DiagnosticReport(mods, patches, assemblies, patchComparison, retargetStatus, envInfo, dllIntegrity, warnings, debugInfo, collectedAt);

            report.Mods.Should().BeSameAs(mods);
            report.Patches.Should().BeSameAs(patches);
            report.Assemblies.Should().BeSameAs(assemblies);
            report.PatchComparison.Should().BeSameAs(patchComparison);
            report.RetargetHarmonyStatus.Should().BeSameAs(retargetStatus);
            report.EnvironmentInfo.Should().BeSameAs(envInfo);
            report.DllIntegrity.Should().BeSameAs(dllIntegrity);
            report.Warnings.Should().BeSameAs(warnings);
            report.DebugInfo.Should().BeSameAs(debugInfo);
            report.CollectedAt.Should().Be(collectedAt);
        }

        [Fact]
        public void Constructor_throws_when_mods_is_null()
        {
            Action act = () => _ = new DiagnosticReport(null, [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("mods");
        }

        [Fact]
        public void Constructor_throws_when_patches_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], null, [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patches");
        }

        [Fact]
        public void Constructor_throws_when_assemblies_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], null,
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("assemblies");
        }

        [Fact]
        public void Constructor_throws_when_patchComparison_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                null,
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("patchComparison");
        }

        [Fact]
        public void Constructor_throws_when_retargetHarmonyStatus_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                new PatchComparisonResult([], 0, 0),
                null, new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("retargetHarmonyStatus");
        }

        [Fact]
        public void Constructor_throws_when_environmentInfo_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), null,
                CreateDefaultDllIntegrityReport(), [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("environmentInfo");
        }

        [Fact]
        public void Constructor_throws_when_dllIntegrity_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                null, [], [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllIntegrity");
        }

        [Fact]
        public void Constructor_throws_when_warnings_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), null, [], DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("warnings");
        }

        [Fact]
        public void Constructor_throws_when_debugInfo_is_null()
        {
            Action act = () => _ = new DiagnosticReport([], [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, ""), new EnvironmentInfo(false, false, false),
                CreateDefaultDllIntegrityReport(), [], null, DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>().WithParameterName("debugInfo");
        }
    }
}
