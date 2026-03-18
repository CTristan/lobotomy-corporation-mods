// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Models.Diagnostics;
using Hemocode.DebugPanel.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.DebugPanel.JsonModels;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class KnownIssuesCheckerTests
    {
        private readonly Mock<IKnownIssuesDatabase> _mockDatabase;
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public KnownIssuesCheckerTests()
        {
            _mockDatabase = new Mock<IKnownIssuesDatabase>();
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockDatabase.Setup(d => d.GetKnownIssues()).Returns([]);
            _mockDatabase.Setup(d => d.DatabaseVersion).Returns("1.0");
            _mockScanner.Setup(s => s.GetBaseModsPath()).Returns("/basemods");
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
        }

        [Fact]
        public void Constructor_throws_when_database_is_null()
        {
            Action act = () => _ = new KnownIssuesChecker(null, [], [], _mockScanner.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("database");
        }

        [Fact]
        public void Constructor_throws_when_detectedMods_is_null()
        {
            Action act = () => _ = new KnownIssuesChecker(_mockDatabase.Object, null, [], _mockScanner.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("detectedMods");
        }

        [Fact]
        public void Constructor_throws_when_loadedAssemblies_is_null()
        {
            Action act = () => _ = new KnownIssuesChecker(_mockDatabase.Object, [], null, _mockScanner.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("loadedAssemblies");
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new KnownIssuesChecker(_mockDatabase.Object, [], [], null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Collect_returns_empty_when_database_is_empty()
        {
            var checker = new KnownIssuesChecker(_mockDatabase.Object, [], [], _mockScanner.Object);

            var result = checker.Collect();

            result.Matches.Should().BeEmpty();
            result.DatabaseVersion.Should().Be("1.0");
        }

        [Fact]
        public void Collect_matches_by_dll_name()
        {
            var issue = new KnownIssueItem
            {
                modName = "BadMod",
                dllName = "BadMod.dll",
                severity = 2,
                description = "Bad mod",
                fixSuggestion = "Remove it",
                wikiLink = "https://wiki",
            };
            _mockDatabase.Setup(d => d.GetKnownIssues()).Returns([issue]);
            _mockScanner.Setup(s => s.FileExists("/basemods/BadMod.dll")).Returns(true);

            var checker = new KnownIssuesChecker(_mockDatabase.Object, [], [], _mockScanner.Object);
            var result = checker.Collect();

            result.Matches.Should().HaveCount(1);
            result.Matches[0].ModName.Should().Be("BadMod");
            result.Matches[0].Severity.Should().Be(FindingSeverity.Error);
            result.Matches[0].MatchedBy.Should().Contain("DLL file");
        }

        [Fact]
        public void Collect_matches_by_assembly_name()
        {
            var issue = new KnownIssueItem
            {
                modName = "TestMod",
                assemblyName = "TestModAssembly",
                severity = 1,
                description = "Test issue",
                fixSuggestion = "Fix it",
                wikiLink = "https://wiki",
            };
            _mockDatabase.Setup(d => d.GetKnownIssues()).Returns([issue]);
            var assemblies = new List<AssemblyInfo> { new("TestModAssembly", "1.0", "/path", false, []) };

            var checker = new KnownIssuesChecker(_mockDatabase.Object, [], assemblies, _mockScanner.Object);
            var result = checker.Collect();

            result.Matches.Should().HaveCount(1);
            result.Matches[0].Severity.Should().Be(FindingSeverity.Warning);
            result.Matches[0].MatchedBy.Should().Contain("Loaded assembly");
        }

        [Fact]
        public void Collect_does_not_match_when_mod_not_present()
        {
            var issue = new KnownIssueItem
            {
                modName = "MissingMod",
                dllName = "MissingMod.dll",
                assemblyName = "MissingMod",
                severity = 2,
                description = "Bad",
                fixSuggestion = "Fix",
                wikiLink = "",
            };
            _mockDatabase.Setup(d => d.GetKnownIssues()).Returns([issue]);

            var checker = new KnownIssuesChecker(_mockDatabase.Object, [], [], _mockScanner.Object);
            var result = checker.Collect();

            result.Matches.Should().BeEmpty();
        }

        [Fact]
        public void Collect_detects_conflict_pairs()
        {
            var issue = new KnownIssueItem
            {
                modName = "ModA",
                assemblyName = "ModA",
                severity = 1,
                description = "Conflicts with ModB",
                fixSuggestion = "Remove one",
                wikiLink = "",
                conflictsWith = new[] { "ModB" },
            };
            _mockDatabase.Setup(d => d.GetKnownIssues()).Returns([issue]);
            var assemblies = new List<AssemblyInfo> { new("ModA", "1.0", "/path", false, []) };
            var mods = new List<DetectedModInfo>
            {
                new("ModA", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "ModA", "", false, 0, 0),
                new("ModB", "1.0", ModSource.Lmm, HarmonyVersion.Harmony1, "ModB", "", false, 0, 0),
            };

            var checker = new KnownIssuesChecker(_mockDatabase.Object, mods, assemblies, _mockScanner.Object);
            var result = checker.Collect();

            result.Matches.Should().Contain(m => m.Description.Contains("Conflict") && m.Description.Contains("ModB"));
        }
    }
}
