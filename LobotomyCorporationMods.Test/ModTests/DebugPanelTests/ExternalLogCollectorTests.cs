// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using LobotomyCorporationMods.DebugPanel.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using Moq;
using Xunit;

// ReSharper disable ObjectCreationAsStatement

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ExternalLogCollectorTests
    {
        private readonly Mock<IFileSystemScanner> _mockScanner;

        public ExternalLogCollectorTests()
        {
            _mockScanner = new Mock<IFileSystemScanner>();
            _mockScanner.Setup(s => s.GetGameRootPath()).Returns("/game");
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns("/home/user");
            _mockScanner.Setup(s => s.GetBaseModsPath()).Returns("/game/LobotomyCorp_Data/BaseMods");
            _mockScanner.Setup(s => s.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);
            _mockScanner.Setup(s => s.GetDirectories(It.IsAny<string>())).Returns([]);
        }

        private ExternalLogCollector CreateCollector()
        {
            return new ExternalLogCollector(_mockScanner.Object);
        }

        [Fact]
        public void Constructor_throws_when_scanner_is_null()
        {
            Action act = () => _ = new ExternalLogCollector(null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("scanner");
        }

        [Fact]
        public void Collect_returns_empty_retarget_log_when_game_root_is_empty()
        {
            _mockScanner.Setup(s => s.GetGameRootPath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_retarget_harmony_log_from_primary_path()
        {
            var primaryPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine("/game", "BepInEx"), "patchers"), "RetargetHarmony"), "logs");
            primaryPath = Path.Combine(primaryPath, "RetargetHarmony.log");
            _mockScanner.Setup(s => s.FileExists(primaryPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(primaryPath)).Returns("retarget content");

            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().Be("retarget content");
        }

        [Fact]
        public void Collect_reads_retarget_harmony_log_from_fallback_path()
        {
            var fallbackPath = Path.Combine(Path.Combine(Path.Combine("/game", "BepInEx"), "patchers"), "RetargetHarmony.log");
            _mockScanner.Setup(s => s.FileExists(fallbackPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(fallbackPath)).Returns("fallback content");

            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().Be("fallback content");
        }

        [Fact]
        public void Collect_returns_empty_retarget_log_when_neither_path_exists()
        {
            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_informative_message_when_retarget_log_is_empty()
        {
            var primaryPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine("/game", "BepInEx"), "patchers"), "RetargetHarmony"), "logs");
            primaryPath = Path.Combine(primaryPath, "RetargetHarmony.log");
            _mockScanner.Setup(s => s.FileExists(primaryPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(primaryPath)).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().Contain("empty");
        }

        [Fact]
        public void Collect_returns_empty_retarget_log_when_read_throws()
        {
            var primaryPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine("/game", "BepInEx"), "patchers"), "RetargetHarmony"), "logs");
            primaryPath = Path.Combine(primaryPath, "RetargetHarmony.log");
            _mockScanner.Setup(s => s.FileExists(primaryPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(primaryPath)).Throws<IOException>();

            var result = CreateCollector().Collect();

            result.RetargetHarmonyLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_bepinex_log_when_game_root_is_empty()
        {
            _mockScanner.Setup(s => s.GetGameRootPath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.BepInExLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_bepinex_log_via_locked_file()
        {
            var logPath = Path.Combine(Path.Combine("/game", "BepInEx"), "LogOutput.log");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Returns("bepinex content");

            var result = CreateCollector().Collect();

            result.BepInExLog.Should().Be("bepinex content");
        }

        [Fact]
        public void Collect_returns_empty_bepinex_log_when_file_not_found()
        {
            var result = CreateCollector().Collect();

            result.BepInExLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_unity_log_via_locked_file()
        {
            var logPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "output_log.txt");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Returns("unity content");

            var result = CreateCollector().Collect();

            result.UnityLog.Should().Be("unity content");
        }

        [Fact]
        public void Collect_returns_empty_unity_log_when_user_profile_is_empty()
        {
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.UnityLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_gameplay_log_via_locked_file()
        {
            var logPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "LobotomyBaseMod"), "Log.txt");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Returns("gameplay content");

            var result = CreateCollector().Collect();

            result.GameplayLog.Should().Be("gameplay content");
        }

        [Fact]
        public void Collect_returns_empty_gameplay_log_when_user_profile_is_empty()
        {
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.GameplayLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_gameplay_log_when_file_not_found()
        {
            var result = CreateCollector().Collect();

            result.GameplayLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_most_recent_save_folder_log()
        {
            var logDir = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "Log");
            var file1 = Path.Combine(logDir, "session1.txt");
            var file2 = Path.Combine(logDir, "session2.txt");
            _mockScanner.Setup(s => s.GetFiles(logDir, "*.txt")).Returns([file1, file2]);
            _mockScanner.Setup(s => s.GetLastWriteTimeUtc(file1)).Returns(new DateTime(2026, 1, 1));
            _mockScanner.Setup(s => s.GetLastWriteTimeUtc(file2)).Returns(new DateTime(2026, 1, 2));
            _mockScanner.Setup(s => s.ReadLockedFile(file2)).Returns("most recent session");

            var result = CreateCollector().Collect();

            result.SaveFolderLog.Should().Be("most recent session");
        }

        [Fact]
        public void Collect_returns_empty_save_folder_log_when_no_files_found()
        {
            var result = CreateCollector().Collect();

            result.SaveFolderLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_save_folder_log_when_user_profile_is_empty()
        {
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.SaveFolderLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_save_folder_log_when_single_file_exists()
        {
            var logDir = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "Log");
            var file1 = Path.Combine(logDir, "session1.txt");
            _mockScanner.Setup(s => s.GetFiles(logDir, "*.txt")).Returns([file1]);
            _mockScanner.Setup(s => s.GetLastWriteTimeUtc(file1)).Returns(new DateTime(2026, 1, 1));
            _mockScanner.Setup(s => s.ReadLockedFile(file1)).Returns("single session");

            var result = CreateCollector().Collect();

            result.SaveFolderLog.Should().Be("single session");
        }

        [Fact]
        public void Collect_reads_lmm_directory_log_from_sibling_directory()
        {
            var parentDir = Path.GetDirectoryName("/game");
            _mockScanner.Setup(s => s.GetDirectories(parentDir!)).Returns(["/LobotomyModManager"]);
            var logPath = Path.Combine(Path.Combine("/LobotomyModManager", "LobotomyModManager_Data"), "Log.txt");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(logPath)).Returns("lmm dir content");

            var result = CreateCollector().Collect();

            result.LmmDirectoryLog.Should().Be("lmm dir content");
        }

        [Fact]
        public void Collect_returns_empty_lmm_directory_log_when_no_sibling_found()
        {
            var parentDir = Path.GetDirectoryName("/game");
            _mockScanner.Setup(s => s.GetDirectories(parentDir!)).Returns(["/SomeOtherDir"]);

            var result = CreateCollector().Collect();

            result.LmmDirectoryLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_lmm_directory_log_when_game_root_is_empty()
        {
            _mockScanner.Setup(s => s.GetGameRootPath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.LmmDirectoryLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_lmm_directory_log_when_search_throws()
        {
            var parentDir = Path.GetDirectoryName("/game");
            _mockScanner.Setup(s => s.GetDirectories(parentDir!)).Throws<IOException>();

            var result = CreateCollector().Collect();

            result.LmmDirectoryLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_lmm_system_log_via_locked_file()
        {
            var logPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "DefaultCompany"), "LobotomyModManager"), "Player.log");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Returns("lmm sys content");

            var result = CreateCollector().Collect();

            result.LmmSystemLog.Should().Be("lmm sys content");
        }

        [Fact]
        public void Collect_returns_empty_lmm_system_log_when_user_profile_is_empty()
        {
            _mockScanner.Setup(s => s.GetUserProfilePath()).Returns(string.Empty);

            var result = CreateCollector().Collect();

            result.LmmSystemLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_lmm_system_log_when_file_not_found()
        {
            var result = CreateCollector().Collect();

            result.LmmSystemLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_reads_basemods_txt_files()
        {
            var baseModsPath = "/game/LobotomyCorp_Data/BaseMods";
            var file1 = Path.Combine(baseModsPath, "error.txt");
            _mockScanner.Setup(s => s.GetFiles(baseModsPath, "*.txt")).Returns([file1]);
            _mockScanner.Setup(s => s.FileExists(file1)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(file1)).Returns("error content");

            var result = CreateCollector().Collect();

            result.BaseModsLog.Should().Contain("error.txt");
            result.BaseModsLog.Should().Contain("error content");
        }

        [Fact]
        public void Collect_combines_multiple_basemods_txt_files()
        {
            var baseModsPath = "/game/LobotomyCorp_Data/BaseMods";
            var file1 = Path.Combine(baseModsPath, "error1.txt");
            var file2 = Path.Combine(baseModsPath, "error2.txt");
            _mockScanner.Setup(s => s.GetFiles(baseModsPath, "*.txt")).Returns([file1, file2]);
            _mockScanner.Setup(s => s.FileExists(file1)).Returns(true);
            _mockScanner.Setup(s => s.FileExists(file2)).Returns(true);
            _mockScanner.Setup(s => s.ReadAllText(file1)).Returns("content1");
            _mockScanner.Setup(s => s.ReadAllText(file2)).Returns("content2");

            var result = CreateCollector().Collect();

            result.BaseModsLog.Should().Contain("error1.txt");
            result.BaseModsLog.Should().Contain("content1");
            result.BaseModsLog.Should().Contain("error2.txt");
            result.BaseModsLog.Should().Contain("content2");
        }

        [Fact]
        public void Collect_returns_empty_basemods_log_when_no_txt_files_found()
        {
            var result = CreateCollector().Collect();

            result.BaseModsLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_basemods_log_when_read_throws()
        {
            var baseModsPath = "/game/LobotomyCorp_Data/BaseMods";
            _mockScanner.Setup(s => s.GetFiles(baseModsPath, "*.txt")).Throws<IOException>();

            var result = CreateCollector().Collect();

            result.BaseModsLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_save_folder_log_when_get_files_throws()
        {
            var logDir = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("/home/user", "AppData"), "LocalLow"), "Project_Moon"), "Lobotomy"), "Log");
            _mockScanner.Setup(s => s.GetFiles(logDir, "*.txt")).Throws<IOException>();

            var result = CreateCollector().Collect();

            result.SaveFolderLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_returns_empty_bepinex_log_when_read_throws()
        {
            var logPath = Path.Combine(Path.Combine("/game", "BepInEx"), "LogOutput.log");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Throws<IOException>();

            var result = CreateCollector().Collect();

            result.BepInExLog.Should().BeEmpty();
        }

        [Fact]
        public void Collect_parses_bepinex_timestamp_from_log_content()
        {
            var logPath = Path.Combine(Path.Combine("/game", "BepInEx"), "LogOutput.log");
            _mockScanner.Setup(s => s.FileExists(logPath)).Returns(true);
            _mockScanner.Setup(s => s.ReadLockedFile(logPath)).Returns("BepInEx 5.4.23.2 - LobotomyCorp (3/15/2026 2:30:00 PM)\n[Message] stuff");

            var result = CreateCollector().Collect();

            result.BepInExLogTimestamp.Should().NotBeNull();
            result.BepInExLogTimestamp!.Value.Month.Should().Be(3);
            result.BepInExLogTimestamp!.Value.Day.Should().Be(15);
            result.BepInExLogTimestamp!.Value.Year.Should().Be(2026);
        }

        [Fact]
        public void Collect_returns_null_bepinex_timestamp_when_log_is_empty()
        {
            var result = CreateCollector().Collect();

            result.BepInExLogTimestamp.Should().BeNull();
        }

        [Fact]
        public void ParseBepInExTimestamp_returns_null_for_malformed_input()
        {
            var result = ExternalLogData.ParseBepInExTimestamp("no parentheses here");

            result.Should().BeNull();
        }

        [Fact]
        public void ParseBepInExTimestamp_returns_null_for_empty_string()
        {
            var result = ExternalLogData.ParseBepInExTimestamp(string.Empty);

            result.Should().BeNull();
        }

        [Fact]
        public void ParseBepInExTimestamp_returns_null_for_null()
        {
            var result = ExternalLogData.ParseBepInExTimestamp(null!);

            result.Should().BeNull();
        }

        [Fact]
        public void ParseBepInExTimestamp_returns_null_for_unparseable_date_in_parentheses()
        {
            var result = ExternalLogData.ParseBepInExTimestamp("BepInEx (not a date)");

            result.Should().BeNull();
        }

        [Fact]
        public void ParseBepInExTimestamp_parses_standard_bepinex_format()
        {
            var result = ExternalLogData.ParseBepInExTimestamp("BepInEx 5.4.23.2 - LobotomyCorp (1/5/2026 9:15:30 AM)");

            result.Should().NotBeNull();
            result!.Value.Year.Should().Be(2026);
            result!.Value.Month.Should().Be(1);
            result!.Value.Day.Should().Be(5);
            result!.Value.Hour.Should().Be(9);
            result!.Value.Minute.Should().Be(15);
        }
    }
}
