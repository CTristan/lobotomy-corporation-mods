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
    public sealed class DllIntegrityReportTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var findings = new List<DllIntegrityFinding>
            {
                new("/path/mod.dll", "mod.dll", FindingSeverity.Info, [], [], false, "", false, "Not modified"),
            };
            var warnings = new List<string> { "test warning" };

            var report = new DllIntegrityReport(
                findings, true, "/backup", true, "/cache/file.dat",
                5, true, 0, warnings, "1 DLLs checked, 0 rewritten");

            report.Findings.Should().BeSameAs(findings);
            report.ShimBackupDirectoryExists.Should().BeTrue();
            report.ShimBackupDirectoryPath.Should().Be("/backup");
            report.InteropCacheExists.Should().BeTrue();
            report.InteropCachePath.Should().Be("/cache/file.dat");
            report.InteropCacheEntryCount.Should().Be(5);
            report.MonoCecilAvailable.Should().BeTrue();
            report.TotalRewrittenCount.Should().Be(0);
            report.Warnings.Should().BeSameAs(warnings);
            report.Summary.Should().Be("1 DLLs checked, 0 rewritten");
        }

        [Fact]
        public void Constructor_throws_when_findings_is_null()
        {
            Action act = () => _ = new DllIntegrityReport(
                null, false, "", false, "", -1, false, 0, [], "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("findings");
        }

        [Fact]
        public void Constructor_throws_when_warnings_is_null()
        {
            Action act = () => _ = new DllIntegrityReport(
                [], false, "", false, "", -1, false, 0, null, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("warnings");
        }

        [Fact]
        public void Constructor_throws_when_summary_is_null()
        {
            Action act = () => _ = new DllIntegrityReport(
                [], false, "", false, "", -1, false, 0, [], null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("summary");
        }

        [Fact]
        public void Constructor_defaults_null_shimBackupDirectoryPath_to_empty_string()
        {
            var report = new DllIntegrityReport(
                [], false, null, false, "", -1, false, 0, [], "summary");

            report.ShimBackupDirectoryPath.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_defaults_null_interopCachePath_to_empty_string()
        {
            var report = new DllIntegrityReport(
                [], false, "", false, null, -1, false, 0, [], "summary");

            report.InteropCachePath.Should().BeEmpty();
        }
    }
}
