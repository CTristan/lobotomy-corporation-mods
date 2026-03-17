// SPDX-License-Identifier: MIT

using System.IO;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Deployment
{
    public sealed class GameRestorerTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly GameRestorer _gameRestorer;
        private readonly string _gamePath = "/test/game/path";
        private readonly string _snapshotPath = "/test/repo/external/snapshots";
        private readonly string _vanillaManagedPath;

        public GameRestorerTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _gameRestorer = new GameRestorer(_mockFileSystem.Object);
            _vanillaManagedPath = Path.Combine(_snapshotPath, "LobotomyCorp_vanilla", "LobotomyCorp_Data", "Managed");

            // By default, vanilla snapshot exists
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_vanillaManagedPath)).Returns(true);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(Path.Combine(_snapshotPath, "LobotomyCorp_vanilla"))).Returns(true);
        }

        [Fact]
        public void RestoreTargeted_deletes_BaseMods_directory_when_it_exists()
        {
            // Arrange
            var baseModsPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(baseModsPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(baseModsPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_BepInEx_directory_when_it_exists()
        {
            // Arrange
            var bepInExPath = Path.Combine(_gamePath, "BepInEx");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(bepInExPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(bepInExPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_and_restores_Managed_directory()
        {
            // Arrange
            var managedPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "Managed");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(managedPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(managedPath, true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyDirectory(_vanillaManagedPath, managedPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_skips_delete_when_directories_and_files_do_not_exist()
        {
            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            _mockFileSystem.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_BepInEx_Shim_Backup_directory_when_it_exists()
        {
            // Arrange
            var shimBackupPath = Path.Combine(_gamePath, "BepInEx_Shim_Backup");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(shimBackupPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(shimBackupPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_doorstop_config_ini_when_it_exists()
        {
            // Arrange
            var filePath = Path.Combine(_gamePath, "doorstop_config.ini");
            _ = _mockFileSystem.Setup(f => f.FileExists(filePath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteFile(filePath), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_winhttp_dll_when_it_exists()
        {
            // Arrange
            var filePath = Path.Combine(_gamePath, "winhttp.dll");
            _ = _mockFileSystem.Setup(f => f.FileExists(filePath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteFile(filePath), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_doorstop_version_when_it_exists()
        {
            // Arrange
            var filePath = Path.Combine(_gamePath, ".doorstop_version");
            _ = _mockFileSystem.Setup(f => f.FileExists(filePath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteFile(filePath), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_throws_when_vanilla_snapshot_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_vanillaManagedPath)).Returns(false);

            // Act & Assert
            _ = Assert.Throws<DirectoryNotFoundException>(() => _gameRestorer.RestoreTargeted(_gamePath, _snapshotPath));

            // Verify no directories were deleted
            _mockFileSystem.Verify(f => f.DeleteDirectory(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void RestoreFull_copies_entire_snapshot_to_game_path()
        {
            // Arrange
            var vanillaGamePath = Path.Combine(_snapshotPath, "LobotomyCorp_vanilla");

            // Act
            _gameRestorer.RestoreFull(_gamePath, _snapshotPath);

            // Assert
            _mockFileSystem.Verify(f => f.CopyDirectory(vanillaGamePath, _gamePath, true), Times.Once);
        }

        [Fact]
        public void RestoreFull_throws_when_vanilla_snapshot_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(Path.Combine(_snapshotPath, "LobotomyCorp_vanilla"))).Returns(false);

            // Act & Assert
            _ = Assert.Throws<DirectoryNotFoundException>(() => _gameRestorer.RestoreFull(_gamePath, _snapshotPath));
        }

        [Fact]
        public void RestoreTargeted_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _gameRestorer.RestoreTargeted(null!, _snapshotPath));
        }

        [Fact]
        public void RestoreFull_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _gameRestorer.RestoreFull(null!, _snapshotPath));
        }
    }
}
