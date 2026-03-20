// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class IssuesAggregatorTests
    {
        private static DiagnosticReport CreateEmptyReport()
        {
            return new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow);
        }

        [Fact]
        public void AggregateIssues_throws_when_report_is_null()
        {
            var aggregator = new IssuesAggregator();

            Action act = () => aggregator.AggregateIssues(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void AggregateIssues_returns_empty_for_clean_report()
        {
            var aggregator = new IssuesAggregator();
            var report = CreateEmptyReport();

            var result = aggregator.AggregateIssues(report);

            result.Should().BeEmpty();
        }

        [Fact]
        public void AggregateIssues_includes_filesystem_issues()
        {
            var aggregator = new IssuesAggregator();
            var filesystemReport = new FilesystemValidationReport(
                [
                    new(FindingSeverity.Error, "Filesystem", "Assembly-CSharp.dll in BaseMods", "Files", "Remove it"),
                ],
                "1 issue");
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                filesystemReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Description.Should().Contain("Assembly-CSharp.dll");
        }

        [Fact]
        public void AggregateIssues_includes_dependency_issues()
        {
            var aggregator = new IssuesAggregator();
            var dependencyReport = new DependencyReport(
                [
                    new(FindingSeverity.Error, "Dependencies", "Missing 12Harmony.dll", "Mods", "Install it"),
                ],
                string.Empty,
                false);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                dependencyReport: dependencyReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Description.Should().Contain("12Harmony.dll");
        }

        [Fact]
        public void AggregateIssues_converts_known_issue_matches()
        {
            var aggregator = new IssuesAggregator();
            var knownIssuesReport = new KnownIssuesReport(
                [
                    new("BadMod", FindingSeverity.Error, "Causes crashes", "Remove it", "https://wiki", "DLL file"),
                ],
                "1.0");
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                knownIssuesReport: knownIssuesReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Description.Should().Contain("BadMod");
            result[0].Category.Should().Be("Known Issue");
        }

        [Fact]
        public void AggregateIssues_converts_error_log_entries()
        {
            var aggregator = new IssuesAggregator();
            var errorLogReport = new ErrorLogReport(
                [
                    new("Herror.txt", "some error", "/path/Herror.txt"),
                ]);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                errorLogReport: errorLogReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Severity.Should().Be(FindingSeverity.Error);
            result[0].Description.Should().Contain("Herror.txt");
            result[0].Description.Should().Contain("some error");
        }

        [Fact]
        public void AggregateIssues_error_log_entries_include_known_file_origin_fix_suggestions()
        {
            var aggregator = new IssuesAggregator();
            var errorLogReport = new ErrorLogReport(
                [
                    new("LMMerror.txt", "map load failed", "/path/LMMerror.txt"),
                    new("LTDerror.txt", "locale error", "/path/LTDerror.txt"),
                    new("Glerror.txt", "global error", "/path/Glerror.txt"),
                    new("DPerror.txt", "deploy error", "/path/DPerror.txt"),
                    new("DllError.txt", "dll error", "/path/DllError.txt"),
                ]);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                errorLogReport: errorLogReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(5);
            result[0].FixSuggestion.Should().Contain("Map/graph loading error");
            result[1].FixSuggestion.Should().Contain("Localization data loading error");
            result[2].FixSuggestion.Should().Contain("Global game manager error");
            result[3].FixSuggestion.Should().Contain("Deploy UI error");
            result[4].FixSuggestion.Should().Contain("Check").And.NotContain("Map/graph");
        }

        [Fact]
        public void AggregateIssues_converts_gameplay_log_errors()
        {
            var aggregator = new IssuesAggregator();
            var gameplayLogErrorReport = new GameplayLogErrorReport(
                [
                    new("TestMod v1.0", "TestMod.dll", "Exception thrown", "", "Herror - TestMod v1.0 / TestMod.dllException thrown"),
                ]);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                gameplayLogErrorReport: gameplayLogErrorReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Severity.Should().Be(FindingSeverity.Error);
            result[0].Category.Should().Be("Mod Load Error");
            result[0].Description.Should().Contain("TestMod v1.0");
            result[0].Description.Should().Contain("Exception thrown");
            result[0].SourceTab.Should().Be("Mods");
        }

        [Fact]
        public void AggregateIssues_gameplay_log_error_uses_raw_line_when_mod_name_empty()
        {
            var aggregator = new IssuesAggregator();
            var gameplayLogErrorReport = new GameplayLogErrorReport(
                [
                    new("", "", "some raw error", "", "Herror - some raw error"),
                ]);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow,
                gameplayLogErrorReport: gameplayLogErrorReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Description.Should().Contain("Herror - some raw error");
        }

        [Fact]
        public void AggregateIssues_converts_missing_patches()
        {
            var aggregator = new IssuesAggregator();
            var patchComparison = new PatchComparisonResult(
                [
                    new("TestMod", "GameClass", "GameMethod", "Postfix", PatchType.Postfix),
                ],
                1,
                0);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                patchComparison,
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                [],
                [],
                DateTime.UtcNow);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Severity.Should().Be(FindingSeverity.Warning);
            result[0].Category.Should().Be("Missing Patch");
        }

        [Fact]
        public void AggregateIssues_includes_non_info_dll_integrity_findings()
        {
            var aggregator = new IssuesAggregator();
            var findings = new List<DllIntegrityFinding>
            {
                new("/path/mod.dll", "mod.dll", FindingSeverity.Warning, [], [], false, "", false, "Modified"),
                new("/path/ok.dll", "ok.dll", FindingSeverity.Info, [], [], false, "", false, "Not modified"),
            };
            var dllIntegrity = new DllIntegrityReport(findings, false, string.Empty, false, string.Empty, -1, false, 0, [], "checked");
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                dllIntegrity,
                [],
                [],
                DateTime.UtcNow);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Description.Should().Contain("mod.dll");
        }

        [Fact]
        public void AggregateIssues_converts_warnings_to_info_severity()
        {
            var aggregator = new IssuesAggregator();
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                ["test warning"],
                [],
                DateTime.UtcNow);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(1);
            result[0].Severity.Should().Be(FindingSeverity.Info);
            result[0].Description.Should().Be("test warning");
        }

        [Fact]
        public void AggregateIssues_sorts_by_severity_descending()
        {
            var aggregator = new IssuesAggregator();
            var filesystemReport = new FilesystemValidationReport(
                [
                    new(FindingSeverity.Warning, "Filesystem", "Minor issue", "Files", "Fix it"),
                ],
                "1 issue");
            var dependencyReport = new DependencyReport(
                [
                    new(FindingSeverity.Error, "Dependencies", "Critical issue", "Mods", "Fix now"),
                ],
                string.Empty,
                false);
            var report = new DiagnosticReport(
                [],
                [],
                [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, "Not detected"),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport([], false, string.Empty, false, string.Empty, -1, false, 0, [], "No findings"),
                ["info warning"],
                [],
                DateTime.UtcNow,
                filesystemReport,
                dependencyReport: dependencyReport);

            var result = aggregator.AggregateIssues(report);

            result.Should().HaveCount(3);
            result[0].Severity.Should().Be(FindingSeverity.Error);
            result[1].Severity.Should().Be(FindingSeverity.Warning);
            result[2].Severity.Should().Be(FindingSeverity.Info);
        }
    }
}
