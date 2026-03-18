// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using Hemocode.DebugPanel.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ErrorLogCollectorTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public ErrorLogCollectorTests()
        {
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockScanner.Setup(s => s.GetBaseModsPath()).Returns("/game/LobotomyCorp_Data/BaseMods");
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new ErrorLogCollector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Collect_returns_empty_report_when_no_error_files_exist()
        {
            var collector = new ErrorLogCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Collect_finds_and_reads_herror_txt()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/Herror.txt")).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText("/game/LobotomyCorp_Data/BaseMods/Herror.txt")).Returns("error content");
            var collector = new ErrorLogCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().HaveCount(1);
            result.Entries[0].FileName.Should().Be("Herror.txt");
            result.Entries[0].Content.Should().Be("error content");
            result.Entries[0].FilePath.Should().Be("/game/LobotomyCorp_Data/BaseMods/Herror.txt");
        }

        [Fact]
        public void Collect_finds_multiple_error_files()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/Herror.txt")).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText("/game/LobotomyCorp_Data/BaseMods/Herror.txt")).Returns("herror");
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/DPerror.txt")).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText("/game/LobotomyCorp_Data/BaseMods/DPerror.txt")).Returns("dperror");
            var collector = new ErrorLogCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().HaveCount(2);
        }

        [Fact]
        public void Collect_handles_read_failure_gracefully()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/GlError.txt")).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText("/game/LobotomyCorp_Data/BaseMods/GlError.txt")).Throws(new InvalidOperationException("read failed"));
            var collector = new ErrorLogCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().HaveCount(1);
            result.Entries[0].FileName.Should().Be("GlError.txt");
            result.Entries[0].Content.Should().Contain("Error reading file");
        }
    }
}
