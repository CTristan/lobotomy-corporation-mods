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
    public sealed class DependencyReportTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var issues = new List<DiagnosticIssue>();
            var report = new DependencyReport(issues, "2.0.1", true);

            report.Issues.Should().BeSameAs(issues);
            report.BaseModVersion.Should().Be("2.0.1");
            report.BaseModListExists.Should().BeTrue();
        }

        [Fact]
        public void Constructor_throws_when_issues_is_null()
        {
            Action act = () => _ = new DependencyReport(null, "1.0", true);

            act.Should().Throw<ArgumentNullException>().WithParameterName("issues");
        }

        [Fact]
        public void Constructor_throws_when_baseModVersion_is_null()
        {
            Action act = () => _ = new DependencyReport([], null, true);

            act.Should().Throw<ArgumentNullException>().WithParameterName("baseModVersion");
        }
    }
}
