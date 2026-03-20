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
    public sealed class GameplayLogErrorReportTests
    {
        [Fact]
        public void Constructor_stores_entries()
        {
            var entries = new List<GameplayLogErrorEntry>
            {
                new("ModName", "Mod.dll", "Error", "", "raw"),
            };

            var report = new GameplayLogErrorReport(entries);

            report.Entries.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_throws_when_entries_is_null()
        {
            Action act = () => _ = new GameplayLogErrorReport(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("entries");
        }
    }
}
