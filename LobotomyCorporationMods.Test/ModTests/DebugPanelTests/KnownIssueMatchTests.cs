// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class KnownIssueMatchTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var match = new KnownIssueMatch("TestMod", FindingSeverity.Error, "Bad mod", "Remove it", "https://wiki", "DLL name");

            match.ModName.Should().Be("TestMod");
            match.Severity.Should().Be(FindingSeverity.Error);
            match.Description.Should().Be("Bad mod");
            match.FixSuggestion.Should().Be("Remove it");
            match.WikiLink.Should().Be("https://wiki");
            match.MatchedBy.Should().Be("DLL name");
        }

        [Fact]
        public void Constructor_throws_when_modName_is_null()
        {
            Action act = () => _ = new KnownIssueMatch(null, FindingSeverity.Info, "desc", "fix", "link", "match");

            act.Should().Throw<ArgumentNullException>().WithParameterName("modName");
        }

        [Fact]
        public void Constructor_throws_when_description_is_null()
        {
            Action act = () => _ = new KnownIssueMatch("mod", FindingSeverity.Info, null, "fix", "link", "match");

            act.Should().Throw<ArgumentNullException>().WithParameterName("description");
        }

        [Fact]
        public void Constructor_throws_when_fixSuggestion_is_null()
        {
            Action act = () => _ = new KnownIssueMatch("mod", FindingSeverity.Info, "desc", null, "link", "match");

            act.Should().Throw<ArgumentNullException>().WithParameterName("fixSuggestion");
        }

        [Fact]
        public void Constructor_throws_when_wikiLink_is_null()
        {
            Action act = () => _ = new KnownIssueMatch("mod", FindingSeverity.Info, "desc", "fix", null, "match");

            act.Should().Throw<ArgumentNullException>().WithParameterName("wikiLink");
        }

        [Fact]
        public void Constructor_throws_when_matchedBy_is_null()
        {
            Action act = () => _ = new KnownIssueMatch("mod", FindingSeverity.Info, "desc", "fix", "link", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("matchedBy");
        }
    }
}
