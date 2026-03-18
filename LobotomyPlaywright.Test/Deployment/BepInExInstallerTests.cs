// SPDX-License-Identifier: MIT

using System.IO;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Deployment
{
    public sealed class BepInExInstallerTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly BepInExInstaller _bepInExInstaller;
        private readonly string _gamePath = "/test/game/path";
        private readonly string _sourcePath = "/test/repo/Harmony2ForLmm/Resources/bepinex";

        public BepInExInstallerTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _bepInExInstaller = new BepInExInstaller(_mockFileSystem.Object);

            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_sourcePath)).Returns(true);
        }

        [Fact]
        public void Install_copies_BepInEx_directory_to_game()
        {
            // Arrange
            var bepInExDir = Path.Combine(_sourcePath, "BepInEx");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(bepInExDir)).Returns(true);

            // Act
            _bepInExInstaller.Install(_gamePath, _sourcePath);

            // Assert
            var destDir = Path.Combine(_gamePath, "BepInEx");
            _mockFileSystem.Verify(f => f.CopyDirectory(bepInExDir, destDir, true), Times.Once);
        }

        [Fact]
        public void Install_copies_doorstop_config_ini()
        {
            // Arrange
            var doorstopConfig = Path.Combine(_sourcePath, "doorstop_config.ini");
            _ = _mockFileSystem.Setup(f => f.FileExists(doorstopConfig)).Returns(true);

            // Act
            _bepInExInstaller.Install(_gamePath, _sourcePath);

            // Assert
            var destDoorstop = Path.Combine(_gamePath, "doorstop_config.ini");
            _mockFileSystem.Verify(f => f.CopyFile(doorstopConfig, destDoorstop, true), Times.Once);
        }

        [Fact]
        public void Install_copies_winhttp_dll()
        {
            // Arrange
            var winhttpDll = Path.Combine(_sourcePath, "winhttp.dll");
            _ = _mockFileSystem.Setup(f => f.FileExists(winhttpDll)).Returns(true);

            // Act
            _bepInExInstaller.Install(_gamePath, _sourcePath);

            // Assert
            var destWinhttp = Path.Combine(_gamePath, "winhttp.dll");
            _mockFileSystem.Verify(f => f.CopyFile(winhttpDll, destWinhttp, true), Times.Once);
        }

        [Fact]
        public void Install_throws_when_source_directory_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_sourcePath)).Returns(false);

            // Act & Assert
            _ = Assert.Throws<DirectoryNotFoundException>(() => _bepInExInstaller.Install(_gamePath, _sourcePath));
        }

        [Fact]
        public void Install_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _bepInExInstaller.Install(null!, _sourcePath));
        }

        [Fact]
        public void Install_skips_optional_files_when_not_present()
        {
            // Arrange - no optional files exist (FileExists returns false by default)

            // Act
            _bepInExInstaller.Install(_gamePath, _sourcePath);

            // Assert - no CopyFile calls for optional files
            _mockFileSystem.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
