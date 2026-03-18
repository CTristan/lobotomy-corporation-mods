// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DllIntegrityFindingTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var onDiskRefs = new List<string> { "0Harmony109" };
            var originalRefs = new List<string> { "0Harmony" };

            var finding = new DllIntegrityFinding(
                "/path/mod.dll", "mod.dll", FindingSeverity.Warning,
                onDiskRefs, originalRefs,
                true, "/backup/mod.dll", true,
                "Rewritten by BepInEx shim (backup available)");

            finding.DllPath.Should().Be("/path/mod.dll");
            finding.DllName.Should().Be("mod.dll");
            finding.Severity.Should().Be(FindingSeverity.Warning);
            finding.OnDiskHarmonyReferences.Should().BeSameAs(onDiskRefs);
            finding.OriginalHarmonyReferences.Should().BeSameAs(originalRefs);
            finding.HasBackup.Should().BeTrue();
            finding.BackupPath.Should().Be("/backup/mod.dll");
            finding.WasRewritten.Should().BeTrue();
            finding.Summary.Should().Be("Rewritten by BepInEx shim (backup available)");
        }

        [Fact]
        public void Constructor_throws_when_dllPath_is_null()
        {
            Action act = () => _ = new DllIntegrityFinding(
                null, "mod.dll", FindingSeverity.Info, [], [], false, "", false, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllPath");
        }

        [Fact]
        public void Constructor_throws_when_dllName_is_null()
        {
            Action act = () => _ = new DllIntegrityFinding(
                "/path", null, FindingSeverity.Info, [], [], false, "", false, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("dllName");
        }

        [Fact]
        public void Constructor_throws_when_onDiskHarmonyReferences_is_null()
        {
            Action act = () => _ = new DllIntegrityFinding(
                "/path", "mod.dll", FindingSeverity.Info, null, [], false, "", false, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("onDiskHarmonyReferences");
        }

        [Fact]
        public void Constructor_throws_when_originalHarmonyReferences_is_null()
        {
            Action act = () => _ = new DllIntegrityFinding(
                "/path", "mod.dll", FindingSeverity.Info, [], null, false, "", false, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("originalHarmonyReferences");
        }

        [Fact]
        public void Constructor_throws_when_summary_is_null()
        {
            Action act = () => _ = new DllIntegrityFinding(
                "/path", "mod.dll", FindingSeverity.Info, [], [], false, "", false, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("summary");
        }

        [Fact]
        public void Constructor_defaults_null_backupPath_to_empty_string()
        {
            var finding = new DllIntegrityFinding(
                "/path", "mod.dll", FindingSeverity.Info, [], [], false, null, false, "summary");

            finding.BackupPath.Should().BeEmpty();
        }
    }
}
