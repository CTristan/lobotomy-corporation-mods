// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AwesomeAssertions;
using DebugPanel.Common.Interfaces;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.Common.Models.Diagnostics;
using LobotomyCorporationMods.Test.Attributes;
using Moq;
using Xunit;

// ReSharper disable ObjectCreationAsStatement

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class LogFileWriterTests
    {
        private readonly Mock<IFileManager> _mockFileManager;
        private readonly Mock<IReportFormatter> _mockFormatter;
        private readonly Mock<IInfoCollector<ExternalLogData>> _mockCollector;

        public LogFileWriterTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockFileManager = fixture.Freeze<Mock<IFileManager>>();
            _mockFormatter = fixture.Freeze<Mock<IReportFormatter>>();
            _mockCollector = fixture.Freeze<Mock<IInfoCollector<ExternalLogData>>>();
        }

        private LogFileWriter CreateWriter()
        {
            return new LogFileWriter(_mockFileManager.Object, _mockFormatter.Object, _mockCollector.Object);
        }

        private static ExternalLogData CreateExternalLogs()
        {
            return new ExternalLogData(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_fileManager_is_null(
            Mock<IReportFormatter> mockFormatter,
            Mock<IInfoCollector<ExternalLogData>> mockCollector)
        {
            Action act = () => _ = new LogFileWriter(null!, mockFormatter.Object, mockCollector.Object);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("fileManager");
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_reportFormatter_is_null(
            Mock<IFileManager> mockFileManager,
            Mock<IInfoCollector<ExternalLogData>> mockCollector)
        {
            Action act = () => _ = new LogFileWriter(mockFileManager.Object, null!, mockCollector.Object);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("reportFormatter");
        }

        [Theory, LobotomyAutoData]
        public void Constructor_throws_when_externalLogCollector_is_null(
            Mock<IFileManager> mockFileManager,
            Mock<IReportFormatter> mockFormatter)
        {
            Action act = () => _ = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("externalLogCollector");
        }

        [Fact]
        public void WriteReport_throws_when_report_is_null()
        {
            var writer = CreateWriter();

            Action act = () => writer.WriteReport(null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void WriteReport_calls_FormatForLogFile_and_WriteAllText()
        {
            var report = CreateReport();
            var externalLogs = CreateExternalLogs();
            _mockCollector.Setup(s => s.Collect()).Returns(externalLogs);
            IList<string> formattedLines = ["line1", "line2", "line3"];
            _mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(formattedLines);
            _mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/fake/" + n);

            var writer = CreateWriter();
            writer.WriteReport(report);

            _mockFormatter.Verify(f => f.FormatForLogFile(report, externalLogs), Times.Once());
            _mockFileManager.Verify(f => f.GetFile(It.IsAny<string>()), Times.Once());
            _mockFileManager.Verify(f => f.EnsureDirectoryExists(It.IsAny<string>()), Times.Once());
            _mockFileManager.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void WriteReport_generates_filename_with_DebugPanel_prefix()
        {
            var report = CreateReport();
            var externalLogs = CreateExternalLogs();
            _mockCollector.Setup(s => s.Collect()).Returns(externalLogs);
            _mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["test"]);
            string? capturedFileName = null;
            _mockFileManager.Setup(f => f.GetFile(It.IsAny<string>()))
                .Callback<string>(name => capturedFileName = name)
                .Returns<string>(n => "/fake/" + n);

            var writer = CreateWriter();
            writer.WriteReport(report);

            _ = capturedFileName.Should().StartWith("Logs/DebugPanel_");
            _ = capturedFileName.Should().EndWith(".log");
        }

        [Fact]
        public void WriteReport_joins_lines_with_newline()
        {
            var report = CreateReport();
            var externalLogs = CreateExternalLogs();
            _mockCollector.Setup(s => s.Collect()).Returns(externalLogs);
            _mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["line1", "line2"]);
            _mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/fake/" + n);
            string? capturedContent = null;
            _mockFileManager.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, content) => capturedContent = content);

            var writer = CreateWriter();
            writer.WriteReport(report);

            _ = capturedContent.Should().Contain("line1");
            _ = capturedContent.Should().Contain("line2");
            _ = capturedContent.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void WriteReport_returns_full_file_path()
        {
            var report = CreateReport();
            var externalLogs = CreateExternalLogs();
            _mockCollector.Setup(s => s.Collect()).Returns(externalLogs);
            _mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["test"]);
            _mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/mods/" + n);

            var writer = CreateWriter();
            var result = writer.WriteReport(report);

            _ = result.Should().StartWith("/mods/Logs/DebugPanel_");
            _ = result.Should().EndWith(".log");
        }

        private static DiagnosticReport CreateReport()
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
    }
}
