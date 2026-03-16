// SPDX-License-Identifier: MIT

using System.IO;
using LobotomyPlaywright.Implementations.Deployment;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Deployment
{
    public sealed class LmmInstallerTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly LmmInstaller _lmmInstaller;
        private readonly string _gamePath = "/test/game/path";
        private readonly string _testdataPath = "/test/repo/testdata";
        private readonly string _lmmSnapshotPath;

        public LmmInstallerTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _lmmInstaller = new LmmInstaller(_mockFileSystem.Object);
            _lmmSnapshotPath = Path.Combine(_testdataPath, "LobotomyCorp_LMM");

            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_lmmSnapshotPath)).Returns(true);
        }

        [Fact]
        public void Install_copies_LMM_Managed_directory_to_game()
        {
            // Act
            _lmmInstaller.Install(_gamePath, _testdataPath);

            // Assert
            var source = Path.Combine(_lmmSnapshotPath, "LobotomyCorp_Data", "Managed");
            var dest = Path.Combine(_gamePath, "LobotomyCorp_Data", "Managed");
            _mockFileSystem.Verify(f => f.CopyDirectory(source, dest, true), Times.Once);
        }

        [Fact]
        public void Install_copies_LMM_BaseMods_directory_when_it_exists()
        {
            // Arrange
            var baseModsSource = Path.Combine(_lmmSnapshotPath, "LobotomyCorp_Data", "BaseMods");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(baseModsSource)).Returns(true);

            // Act
            _lmmInstaller.Install(_gamePath, _testdataPath);

            // Assert
            var dest = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            _mockFileSystem.Verify(f => f.CopyDirectory(baseModsSource, dest, true), Times.Once);
        }

        [Fact]
        public void Install_skips_BaseMods_copy_when_directory_does_not_exist()
        {
            // Arrange - DirectoryExists for BaseMods returns false by default

            // Act
            _lmmInstaller.Install(_gamePath, _testdataPath);

            // Assert
            var baseModsDest = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), baseModsDest, true), Times.Never);
        }

        [Fact]
        public void Install_throws_when_LMM_snapshot_directory_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_lmmSnapshotPath)).Returns(false);

            // Act & Assert
            _ = Assert.Throws<DirectoryNotFoundException>(() => _lmmInstaller.Install(_gamePath, _testdataPath));
        }

        [Fact]
        public void Install_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _lmmInstaller.Install(null!, _testdataPath));
        }
    }
}
