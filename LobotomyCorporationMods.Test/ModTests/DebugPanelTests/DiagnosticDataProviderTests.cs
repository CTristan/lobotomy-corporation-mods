// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DiagnosticDataProviderTests
    {
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

        private static ExternalLogData CreateTestExternalLogs()
        {
            return new ExternalLogData("retarget log", "bepinex log", "unity log", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        [Fact]
        public void Constructor_throws_when_reportBuilder_is_null()
        {
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();

            Action act = () => _ = new DiagnosticDataProvider(null, mockCollector.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("reportBuilder");
        }

        [Fact]
        public void Constructor_throws_when_externalLogCollector_is_null()
        {
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();

            Action act = () => _ = new DiagnosticDataProvider(mockBuilder.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("externalLogCollector");
        }

        [Fact]
        public void GetFullReport_returns_report_from_builder()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetFullReport();

            result.Should().BeSameAs(expectedReport);
        }

        [Fact]
        public void GetFullReport_always_rebuilds_report()
        {
            var firstReport = CreateTestReport();
            var secondReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.SetupSequence(b => b.BuildReport())
                .Returns(firstReport)
                .Returns(secondReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result1 = provider.GetFullReport();
            var result2 = provider.GetFullReport();

            result1.Should().BeSameAs(firstReport);
            result2.Should().BeSameAs(secondReport);
        }

        [Fact]
        public void GetMods_returns_mods_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetMods();

            result.Should().BeSameAs(expectedReport.Mods);
        }

        [Fact]
        public void GetPatches_returns_patches_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetPatches();

            result.Should().BeSameAs(expectedReport.Patches);
        }

        [Fact]
        public void GetAssemblies_returns_assemblies_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetAssemblies();

            result.Should().BeSameAs(expectedReport.Assemblies);
        }

        [Fact]
        public void GetPatchComparison_returns_patchComparison_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetPatchComparison();

            result.Should().BeSameAs(expectedReport.PatchComparison);
        }

        [Fact]
        public void GetRetargetHarmonyStatus_returns_retargetHarmonyStatus_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetRetargetHarmonyStatus();

            result.Should().BeSameAs(expectedReport.RetargetHarmonyStatus);
        }

        [Fact]
        public void GetEnvironmentInfo_returns_environmentInfo_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetEnvironmentInfo();

            result.Should().BeSameAs(expectedReport.EnvironmentInfo);
        }

        [Fact]
        public void GetDllIntegrity_returns_dllIntegrity_from_cached_or_built_report()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetDllIntegrity();

            result.Should().BeSameAs(expectedReport.DllIntegrity);
        }

        [Fact]
        public void GetExternalLogs_returns_logs_from_collector()
        {
            var expectedLogs = CreateTestExternalLogs();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            mockCollector.Setup(s => s.Collect()).Returns(expectedLogs);
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            var result = provider.GetExternalLogs();

            result.Should().BeSameAs(expectedLogs);
        }

        [Fact]
        public void GetMods_uses_cached_report_on_second_call()
        {
            var expectedReport = CreateTestReport();
            var mockBuilder = new Mock<IDiagnosticReportBuilder>();
            mockBuilder.Setup(b => b.BuildReport()).Returns(expectedReport);
            var mockCollector = new Mock<IInfoCollector<ExternalLogData>>();
            var provider = new DiagnosticDataProvider(mockBuilder.Object, mockCollector.Object);

            provider.GetMods();
            provider.GetPatches();

            mockBuilder.Verify(b => b.BuildReport(), Times.Once);
        }
    }
}
