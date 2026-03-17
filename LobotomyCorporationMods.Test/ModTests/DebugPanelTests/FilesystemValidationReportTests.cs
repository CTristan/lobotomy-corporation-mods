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
    public sealed class FilesystemValidationReportTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var issues = new List<DiagnosticIssue>();
            var report = new FilesystemValidationReport(issues, "No issues");

            report.Issues.Should().BeSameAs(issues);
            report.Summary.Should().Be("No issues");
        }

        [Fact]
        public void Constructor_throws_when_issues_is_null()
        {
            Action act = () => _ = new FilesystemValidationReport(null, "summary");

            act.Should().Throw<ArgumentNullException>().WithParameterName("issues");
        }

        [Fact]
        public void Constructor_throws_when_summary_is_null()
        {
            Action act = () => _ = new FilesystemValidationReport([], null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("summary");
        }
    }
}
