// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DiagnosticIssueTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var issue = new DiagnosticIssue(FindingSeverity.Warning, "Filesystem", "Test description", "Files", "Fix this");

            issue.Severity.Should().Be(FindingSeverity.Warning);
            issue.Category.Should().Be("Filesystem");
            issue.Description.Should().Be("Test description");
            issue.SourceTab.Should().Be("Files");
            issue.FixSuggestion.Should().Be("Fix this");
        }

        [Fact]
        public void Constructor_throws_when_category_is_null()
        {
            Action act = () => _ = new DiagnosticIssue(FindingSeverity.Info, null, "desc", "tab", "fix");

            act.Should().Throw<ArgumentNullException>().WithParameterName("category");
        }

        [Fact]
        public void Constructor_throws_when_description_is_null()
        {
            Action act = () => _ = new DiagnosticIssue(FindingSeverity.Info, "cat", null, "tab", "fix");

            act.Should().Throw<ArgumentNullException>().WithParameterName("description");
        }

        [Fact]
        public void Constructor_throws_when_sourceTab_is_null()
        {
            Action act = () => _ = new DiagnosticIssue(FindingSeverity.Info, "cat", "desc", null, "fix");

            act.Should().Throw<ArgumentNullException>().WithParameterName("sourceTab");
        }

        [Fact]
        public void Constructor_throws_when_fixSuggestion_is_null()
        {
            Action act = () => _ = new DiagnosticIssue(FindingSeverity.Info, "cat", "desc", "tab", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("fixSuggestion");
        }
    }
}
