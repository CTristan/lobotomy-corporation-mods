// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using Moq;
using Xunit;

// ReSharper disable ObjectCreationAsStatement

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class LogFileWriterTests
    {
        [Fact]
        public void Constructor_throws_when_fileManager_is_null()
        {
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();

            Action act = () => _ = new LogFileWriter(null!, mockFormatter.Object, mockExternalLogSource.Object);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("fileManager");
        }

        [Fact]
        public void Constructor_throws_when_reportFormatter_is_null()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();

            Action act = () => _ = new LogFileWriter(mockFileManager.Object, null!, mockExternalLogSource.Object);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("reportFormatter");
        }

        [Fact]
        public void Constructor_throws_when_externalLogSource_is_null()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();

            Action act = () => _ = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("externalLogSource");
        }

        [Fact]
        public void WriteReport_throws_when_report_is_null()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();
            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, mockExternalLogSource.Object);

            Action act = () => writer.WriteReport(null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void WriteReport_calls_FormatForLogFile_and_WriteAllText()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();
            var report = CreateReport();
            var externalLogs = new ExternalLogData(string.Empty, string.Empty, string.Empty);
            mockExternalLogSource.Setup(s => s.GetExternalLogs()).Returns(externalLogs);
            IList<string> formattedLines = ["line1", "line2", "line3"];
            mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(formattedLines);
            mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/fake/" + n);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, mockExternalLogSource.Object);
            writer.WriteReport(report);

            mockFormatter.Verify(f => f.FormatForLogFile(report, externalLogs), Times.Once());
            mockFileManager.Verify(f => f.GetFile(It.IsAny<string>()), Times.Once());
            mockFileManager.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void WriteReport_generates_filename_with_DebugPanel_prefix()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();
            var report = CreateReport();
            var externalLogs = new ExternalLogData(string.Empty, string.Empty, string.Empty);
            mockExternalLogSource.Setup(s => s.GetExternalLogs()).Returns(externalLogs);
            mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["test"]);
            string? capturedFileName = null;
            mockFileManager.Setup(f => f.GetFile(It.IsAny<string>()))
                .Callback<string>(name => capturedFileName = name)
                .Returns<string>(n => "/fake/" + n);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, mockExternalLogSource.Object);
            writer.WriteReport(report);

            _ = capturedFileName.Should().StartWith("DebugPanel_");
            _ = capturedFileName.Should().EndWith(".log");
        }

        [Fact]
        public void WriteReport_joins_lines_with_newline()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();
            var report = CreateReport();
            var externalLogs = new ExternalLogData(string.Empty, string.Empty, string.Empty);
            mockExternalLogSource.Setup(s => s.GetExternalLogs()).Returns(externalLogs);
            mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["line1", "line2"]);
            mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/fake/" + n);
            string? capturedContent = null;
            mockFileManager.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, content) => capturedContent = content);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, mockExternalLogSource.Object);
            writer.WriteReport(report);

            _ = capturedContent.Should().Contain("line1");
            _ = capturedContent.Should().Contain("line2");
            _ = capturedContent.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void WriteReport_returns_full_file_path()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var mockExternalLogSource = new Mock<IExternalLogSource>();
            var report = CreateReport();
            var externalLogs = new ExternalLogData(string.Empty, string.Empty, string.Empty);
            mockExternalLogSource.Setup(s => s.GetExternalLogs()).Returns(externalLogs);
            mockFormatter.Setup(f => f.FormatForLogFile(report, externalLogs)).Returns(["test"]);
            mockFileManager.Setup(f => f.GetFile(It.IsAny<string>())).Returns<string>(n => "/mods/" + n);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object, mockExternalLogSource.Object);
            var result = writer.WriteReport(report);

            _ = result.Should().StartWith("/mods/DebugPanel_");
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
