// SPDX-License-Identifier: MIT

#region

using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using DebugPanel.Common.Models.Diagnostics;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using LobotomyCorporationMods.Test.Attributes;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class DiagnosticDataProviderTests
    {
        private readonly Mock<IDiagnosticReportBuilder> _mockBuilder;
        private readonly Mock<IInfoCollector<ExternalLogData>> _mockCollector;
        private readonly DiagnosticDataProvider _provider;

        public DiagnosticDataProviderTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockBuilder = fixture.Freeze<Mock<IDiagnosticReportBuilder>>();
            _mockCollector = fixture.Freeze<Mock<IInfoCollector<ExternalLogData>>>();
            _mockBuilder.Setup(b => b.BuildReport()).Returns(CreateTestReport());
            _provider = new DiagnosticDataProvider(_mockBuilder.Object, _mockCollector.Object);
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

        private static ExternalLogData CreateTestExternalLogs()
        {
            return new ExternalLogData("retarget log", "bepinex log", "unity log", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_reportBuilder_is_null(Mock<IInfoCollector<ExternalLogData>> mockCollector)
        {
            Action act = () => _ = new DiagnosticDataProvider(null, mockCollector.Object);

            act.Should().Throw<ArgumentNullException>().WithParameterName("reportBuilder");
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_externalLogCollector_is_null(Mock<IDiagnosticReportBuilder> mockBuilder)
        {
            Action act = () => _ = new DiagnosticDataProvider(mockBuilder.Object, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("externalLogCollector");
        }

        [Fact]
        public void GetFullReport_returns_report_from_builder()
        {
            var result = _provider.GetFullReport();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetFullReport_always_rebuilds_report()
        {
            var firstReport = CreateTestReport();
            var secondReport = CreateTestReport();
            _mockBuilder.SetupSequence(b => b.BuildReport())
                .Returns(firstReport)
                .Returns(secondReport);
            var provider = new DiagnosticDataProvider(_mockBuilder.Object, _mockCollector.Object);

            var result1 = provider.GetFullReport();
            var result2 = provider.GetFullReport();

            result1.Should().BeSameAs(firstReport);
            result2.Should().BeSameAs(secondReport);
        }

        [Fact]
        public void GetMods_returns_mods_from_cached_or_built_report()
        {
            var result = _provider.GetMods();

            result.Should().BeSameAs(_provider.GetFullReport().Mods);
        }

        [Fact]
        public void GetPatches_returns_patches_from_cached_or_built_report()
        {
            var result = _provider.GetPatches();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetAssemblies_returns_assemblies_from_cached_or_built_report()
        {
            var result = _provider.GetAssemblies();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetPatchComparison_returns_patchComparison_from_cached_or_built_report()
        {
            var result = _provider.GetPatchComparison();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetRetargetHarmonyStatus_returns_retargetHarmonyStatus_from_cached_or_built_report()
        {
            var result = _provider.GetRetargetHarmonyStatus();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetEnvironmentInfo_returns_environmentInfo_from_cached_or_built_report()
        {
            var result = _provider.GetEnvironmentInfo();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetDllIntegrity_returns_dllIntegrity_from_cached_or_built_report()
        {
            var result = _provider.GetDllIntegrity();

            result.Should().NotBeNull();
        }

        [Fact]
        public void GetExternalLogs_returns_logs_from_collector()
        {
            var expectedLogs = CreateTestExternalLogs();
            _mockCollector.Setup(s => s.Collect()).Returns(expectedLogs);

            var result = _provider.GetExternalLogs();

            result.Should().BeSameAs(expectedLogs);
        }

        [Fact]
        public void GetMods_uses_cached_report_on_second_call()
        {
            _provider.GetMods();
            _provider.GetPatches();

            _mockBuilder.Verify(b => b.BuildReport(), Times.Once);
        }
    }
}
