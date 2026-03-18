// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class GameplayLogErrorCollectorTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public GameplayLogErrorCollectorTests()
        {
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns("/users/test");
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new GameplayLogErrorCollector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Collect_returns_empty_report_when_log_file_does_not_exist()
        {
            var collector = new GameplayLogErrorCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_report_when_user_profile_is_empty()
        {
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns(string.Empty);
            var collector = new GameplayLogErrorCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().BeEmpty();
        }

        [Fact]
        public void Collect_parses_herror_entries_from_gameplay_log()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(It.IsAny<string>()))
                .Returns("Herror - TestMod v1.0 / TestMod.dllSome exception");
            var collector = new GameplayLogErrorCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().HaveCount(1);
            result.Entries[0].ModName.Should().Be("TestMod v1.0");
            result.Entries[0].DllName.Should().Be("TestMod.dll");
        }

        [Fact]
        public void Collect_returns_empty_report_when_read_throws_exception()
        {
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(It.IsAny<string>())).Throws(new System.IO.IOException("File locked"));
            var collector = new GameplayLogErrorCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Entries.Should().BeEmpty();
        }
    }
}
