// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class LogFileWriterTests
    {
        [Fact]
        public void Constructor_throws_when_fileManager_is_null()
        {
            var mockFormatter = new Mock<IReportFormatter>();

            Action act = () => _ = new LogFileWriter(null!, mockFormatter.Object);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("fileManager");
        }

        [Fact]
        public void Constructor_throws_when_reportFormatter_is_null()
        {
            var mockFileManager = new Mock<IFileManager>();

            Action act = () => _ = new LogFileWriter(mockFileManager.Object, null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("reportFormatter");
        }

        [Fact]
        public void WriteReport_throws_when_report_is_null()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object);

            Action act = () => writer.WriteReport(null!);

            _ = act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [Fact]
        public void WriteReport_calls_FormatForLogFile_and_WriteAllText()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var report = CreateReport();
            IList<string> formattedLines = ["line1", "line2", "line3"];
            mockFormatter.Setup(f => f.FormatForLogFile(report)).Returns(formattedLines);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object);
            writer.WriteReport(report);

            mockFormatter.Verify(f => f.FormatForLogFile(report), Times.Once());
            mockFileManager.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Fact]
        public void WriteReport_generates_filename_with_DebugPanel_prefix()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var report = CreateReport();
            mockFormatter.Setup(f => f.FormatForLogFile(report)).Returns(["test"]);
            string? capturedFileName = null;
            mockFileManager.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((name, _) => capturedFileName = name);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object);
            writer.WriteReport(report);

            _ = capturedFileName.Should().StartWith("DebugPanel_");
            _ = capturedFileName.Should().EndWith(".log");
        }

        [Fact]
        public void WriteReport_joins_lines_with_newline()
        {
            var mockFileManager = new Mock<IFileManager>();
            var mockFormatter = new Mock<IReportFormatter>();
            var report = CreateReport();
            mockFormatter.Setup(f => f.FormatForLogFile(report)).Returns(["line1", "line2"]);
            string? capturedContent = null;
            mockFileManager.Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, content) => capturedContent = content);

            var writer = new LogFileWriter(mockFileManager.Object, mockFormatter.Object);
            writer.WriteReport(report);

            _ = capturedContent.Should().Contain("line1");
            _ = capturedContent.Should().Contain("line2");
            _ = capturedContent.Should().Contain(Environment.NewLine);
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
