// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Hemocode.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class KnownIssuesReportTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var matches = new List<KnownIssueMatch>();
            var report = new KnownIssuesReport(matches, "1.0");

            report.Matches.Should().BeSameAs(matches);
            report.DatabaseVersion.Should().Be("1.0");
        }

        [Fact]
        public void Constructor_throws_when_matches_is_null()
        {
            Action act = () => _ = new KnownIssuesReport(null, "1.0");

            act.Should().Throw<ArgumentNullException>().WithParameterName("matches");
        }

        [Fact]
        public void Constructor_throws_when_databaseVersion_is_null()
        {
            Action act = () => _ = new KnownIssuesReport([], null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("databaseVersion");
        }
    }
}
