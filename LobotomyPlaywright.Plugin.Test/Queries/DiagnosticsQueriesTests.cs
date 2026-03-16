// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using LobotomyCorporationMods.Playwright.Queries;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Queries
{
    public sealed class DiagnosticsQueriesTests : IDisposable
    {
        public DiagnosticsQueriesTests()
        {
            DiagnosticDataRegistry.Unregister();
        }

        public void Dispose()
        {
            DiagnosticDataRegistry.Unregister();
        }

        private static DiagnosticReport CreateTestReport()
        {
            return new DiagnosticReport(
                [], [], [],
                new PatchComparisonResult([], 0, 0),
                new RetargetHarmonyStatus(false, false, false, string.Empty),
                new EnvironmentInfo(false, false, false),
                new DllIntegrityReport(
                    [], false, string.Empty, false, string.Empty,
                    -1, false, 0, [], "No findings"),
                [], [],
                DateTime.UtcNow);
        }

        private static void RegisterMockProvider(DiagnosticReport report = null, ExternalLogData externalLogs = null)
        {
            var testReport = report ?? CreateTestReport();
            var testLogs = externalLogs ?? new ExternalLogData(string.Empty, string.Empty, string.Empty);
            var mockProvider = new Mock<IDiagnosticDataProvider>();
            mockProvider.Setup(p => p.GetFullReport()).Returns(testReport);
            mockProvider.Setup(p => p.GetMods()).Returns(testReport.Mods);
            mockProvider.Setup(p => p.GetPatches()).Returns(testReport.Patches);
            mockProvider.Setup(p => p.GetAssemblies()).Returns(testReport.Assemblies);
            mockProvider.Setup(p => p.GetPatchComparison()).Returns(testReport.PatchComparison);
            mockProvider.Setup(p => p.GetRetargetHarmonyStatus()).Returns(testReport.RetargetHarmonyStatus);
            mockProvider.Setup(p => p.GetEnvironmentInfo()).Returns(testReport.EnvironmentInfo);
            mockProvider.Setup(p => p.GetDllIntegrity()).Returns(testReport.DllIntegrity);
            mockProvider.Setup(p => p.GetExternalLogs()).Returns(testLogs);

            DiagnosticDataRegistry.Register(mockProvider.Object);
        }

        [Fact]
        public void HandleQuery_throws_when_parameters_is_null()
        {
            Action act = () => DiagnosticsQueries.HandleQuery("req-1", null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void HandleQuery_returns_error_when_debugpanel_not_loaded()
        {
            var parameters = new Dictionary<string, object>();

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("error");
            result.Code.Should().Be("DIAGNOSTICS_NOT_AVAILABLE");
        }

        [Fact]
        public void HandleQuery_returns_full_report_when_no_section_specified()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object>();

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
            result.DataObject.Should().NotBeNull();
        }

        [Fact]
        public void HandleQuery_returns_full_report_for_full_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "full" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_mods_for_mods_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "mods" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_patches_for_patches_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "patches" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_assemblies_for_assemblies_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "assemblies" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_patch_comparison_for_patch_comparison_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "patch-comparison" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_retarget_for_retarget_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "retarget" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_environment_for_environment_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "environment" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_dll_integrity_for_dll_integrity_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "dll-integrity" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_external_logs_for_external_logs_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "external-logs" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_returns_error_for_unknown_section()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "nonexistent" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("error");
            result.Code.Should().Be("UNKNOWN_SECTION");
        }

        [Fact]
        public void HandleQuery_section_is_case_insensitive()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object> { { "section", "FULL" } };

            var result = DiagnosticsQueries.HandleQuery("req-1", parameters);

            result.Status.Should().Be("ok");
        }

        [Fact]
        public void HandleQuery_preserves_request_id_in_response()
        {
            RegisterMockProvider();
            var parameters = new Dictionary<string, object>();

            var result = DiagnosticsQueries.HandleQuery("my-request-id", parameters);

            result.Id.Should().Be("my-request-id");
        }

        [Fact]
        public void HandleQuery_preserves_request_id_in_error_response()
        {
            var parameters = new Dictionary<string, object>();

            var result = DiagnosticsQueries.HandleQuery("my-request-id", parameters);

            result.Id.Should().Be("my-request-id");
        }
    }
}
