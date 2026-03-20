// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Implementations;
using DebugPanel.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class FilesystemValidationCollectorTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public FilesystemValidationCollectorTests()
        {
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockScanner.Setup(s => s.GetBaseModsPath()).Returns("/game/LobotomyCorp_Data/BaseMods");
            _mockScanner.Setup(s => s.GetSaveDataPath()).Returns("/short/path");
            _mockScanner.Setup(s => s.DirectoryExists(It.IsAny<string>())).Returns(true);
            _mockScanner.Setup(s => s.FileExists(It.IsAny<string>())).Returns(false);
            _mockScanner.Setup(s => s.GetDirectories(It.IsAny<string>())).Returns([]);
            _mockScanner.Setup(s => s.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new FilesystemValidationCollector(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Collect_returns_empty_issues_when_no_problems_detected()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().BeEmpty();
            result.Summary.Should().Contain("0 filesystem issue(s) found");
        }

        [Fact]
        public void Collect_detects_assembly_csharp_in_basemods()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/Assembly-CSharp.dll")).Returns(true);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Error);
            result.Issues[0].Description.Should().Contain("Assembly-CSharp.dll");
        }

        [Fact]
        public void Collect_detects_lmm_executables_in_basemods()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/LobotomyModManager.exe")).Returns(true);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Error);
            result.Issues[0].Description.Should().Contain("LobotomyModManager.exe");
        }

        [Fact]
        public void Collect_detects_basemodlib_in_basemods()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/LobotomyBaseModLib.dll")).Returns(true);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Error);
        }

        [Fact]
        public void Collect_detects_double_folder_nesting()
        {
            _mockScanner.Setup(s => s.GetDirectories("/game/LobotomyCorp_Data/BaseMods")).Returns(["/game/LobotomyCorp_Data/BaseMods/MyMod"]);
            _mockScanner.Setup(s => s.GetDirectories("/game/LobotomyCorp_Data/BaseMods/MyMod")).Returns(["/game/LobotomyCorp_Data/BaseMods/MyMod/MyMod"]);
            _mockScanner.Setup(s => s.GetFiles("/game/LobotomyCorp_Data/BaseMods/MyMod/MyMod", "*.dll")).Returns(["mod.dll"]);
            _mockScanner.Setup(s => s.GetFiles("/game/LobotomyCorp_Data/BaseMods/MyMod", "*.dll")).Returns([]);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Warning);
            result.Issues[0].Description.Should().Contain("Double-folder nesting");
        }

        [Fact]
        public void Collect_does_not_flag_double_nesting_when_outer_has_dlls()
        {
            _mockScanner.Setup(s => s.GetDirectories("/game/LobotomyCorp_Data/BaseMods")).Returns(["/game/LobotomyCorp_Data/BaseMods/MyMod"]);
            _mockScanner.Setup(s => s.GetDirectories("/game/LobotomyCorp_Data/BaseMods/MyMod")).Returns(["/game/LobotomyCorp_Data/BaseMods/MyMod/Sub"]);
            _mockScanner.Setup(s => s.GetFiles("/game/LobotomyCorp_Data/BaseMods/MyMod/Sub", "*.dll")).Returns(["sub.dll"]);
            _mockScanner.Setup(s => s.GetFiles("/game/LobotomyCorp_Data/BaseMods/MyMod", "*.dll")).Returns(["mod.dll"]);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().BeEmpty();
        }

        [Fact]
        public void Collect_detects_long_save_data_path()
        {
            var longPath = new string('a', 261);
            _mockScanner.Setup(s => s.GetSaveDataPath()).Returns(longPath);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize(It.IsAny<string>())).Returns(100);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Warning);
            result.Issues[0].Description.Should().Contain("260 characters");
        }

        [Fact]
        public void Collect_detects_missing_basemodlist()
        {
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Warning);
            result.Issues[0].Description.Should().Contain("BaseModList_v2.xml not found");
        }

        [Fact]
        public void Collect_detects_empty_basemodlist()
        {
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(true);
            _mockScanner.Setup(s => s.GetFileSize("/game/LobotomyCorp_Data/BaseMods/BaseModList_v2.xml")).Returns(0);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().HaveCount(1);
            result.Issues[0].Severity.Should().Be(FindingSeverity.Warning);
            result.Issues[0].Description.Should().Contain("empty");
        }

        [Fact]
        public void Collect_skips_lmm_check_when_basemods_directory_does_not_exist()
        {
            _mockScanner.Setup(s => s.DirectoryExists("/game/LobotomyCorp_Data/BaseMods")).Returns(false);
            _mockScanner.Setup(s => s.FileExists("/game/LobotomyCorp_Data/BaseMods/LobotomyModManager.exe")).Returns(true);
            var collector = new FilesystemValidationCollector(_mockScanner.Object);

            var result = collector.Collect();

            result.Issues.Should().NotContain(i => i.Description.Contains("LobotomyModManager.exe"));
        }
    }
}
