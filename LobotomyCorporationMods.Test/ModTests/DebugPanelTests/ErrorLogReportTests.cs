// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using DebugPanel.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ErrorLogReportTests
    {
        [Fact]
        public void Constructor_stores_entries()
        {
            var entries = new List<ErrorLogEntry>();
            var report = new ErrorLogReport(entries);

            report.Entries.Should().BeSameAs(entries);
        }

        [Fact]
        public void Constructor_throws_when_entries_is_null()
        {
            Action act = () => _ = new ErrorLogReport(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("entries");
        }
    }
}
