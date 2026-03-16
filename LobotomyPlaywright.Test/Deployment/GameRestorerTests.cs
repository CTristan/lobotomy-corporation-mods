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
        private readonly string _testdataPath = "/test/repo/testdata";

        public GameRestorerTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _gameRestorer = new GameRestorer(_mockFileSystem.Object);
        }

        [Fact]
        public void RestoreTargeted_deletes_BaseMods_directory_when_it_exists()
        {
            // Arrange
            var baseModsPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(baseModsPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _testdataPath);

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
            _gameRestorer.RestoreTargeted(_gamePath, _testdataPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(bepInExPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_deletes_and_restores_Managed_directory()
        {
            // Arrange
            var managedPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "Managed");
            var testdataManagedPath = Path.Combine(_testdataPath, "LobotomyCorp_vanilla", "LobotomyCorp_Data", "Managed");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(managedPath)).Returns(true);

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _testdataPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(managedPath, true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyDirectory(testdataManagedPath, managedPath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_skips_delete_when_directories_do_not_exist()
        {
            // Arrange - DirectoryExists returns false by default

            // Act
            _gameRestorer.RestoreTargeted(_gamePath, _testdataPath);

            // Assert
            _mockFileSystem.Verify(f => f.DeleteDirectory(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public void RestoreFull_copies_entire_testdata_to_game_path()
        {
            // Arrange
            var testdataGamePath = Path.Combine(_testdataPath, "LobotomyCorp_vanilla");

            // Act
            _gameRestorer.RestoreFull(_gamePath, _testdataPath);

            // Assert
            _mockFileSystem.Verify(f => f.CopyDirectory(testdataGamePath, _gamePath, true), Times.Once);
        }

        [Fact]
        public void RestoreTargeted_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _gameRestorer.RestoreTargeted(null!, _testdataPath));
        }

        [Fact]
        public void RestoreFull_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _gameRestorer.RestoreFull(null!, _testdataPath));
        }
    }
}
