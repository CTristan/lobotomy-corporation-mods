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
        private readonly string _lmmSourcePath = "/test/snapshots/LobotomyModManager";
        private readonly string _patchFilesPath;
        private readonly string _gameManagedPath;

        public LmmInstallerTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _lmmInstaller = new LmmInstaller(_mockFileSystem.Object);
            _patchFilesPath = Path.Combine(_lmmSourcePath, "LobotomyModManager_Data", "PatchFiles");
            _gameManagedPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "Managed");

            // Default: PatchFiles directory exists
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_patchFilesPath)).Returns(true);

            // Default: patched DLL exists
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_patchFilesPath, "Assembly-CSharp_patched.dll"))).Returns(true);

            // Default: no extra DLLs in PatchFiles
            _ = _mockFileSystem.Setup(f => f.GetFiles(_patchFilesPath, "*.dll")).Returns([]);
        }

        [Fact]
        public void Install_copies_patched_Assembly_CSharp_to_Managed()
        {
            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            var source = Path.Combine(_patchFilesPath, "Assembly-CSharp_patched.dll");
            var dest = Path.Combine(_gameManagedPath, "Assembly-CSharp.dll");
            _mockFileSystem.Verify(f => f.CopyFile(source, dest, true), Times.Once);
        }

        [Fact]
        public void Install_copies_supporting_DLLs_from_PatchFiles()
        {
            // Arrange
            var harmonyDll = Path.Combine(_patchFilesPath, "0Harmony.dll");
            var baseModLibDll = Path.Combine(_patchFilesPath, "LobotomyBaseModLib.dll");
            _ = _mockFileSystem.Setup(f => f.GetFiles(_patchFilesPath, "*.dll"))
                .Returns(new[] { harmonyDll, baseModLibDll });

            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            _mockFileSystem.Verify(f => f.CopyFile(harmonyDll, Path.Combine(_gameManagedPath, "0Harmony.dll"), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(baseModLibDll, Path.Combine(_gameManagedPath, "LobotomyBaseModLib.dll"), true), Times.Once);
        }

        [Fact]
        public void Install_excludes_vanilla_Assembly_CSharp_and_Lobotomypatch_DLLs()
        {
            // Arrange
            var vanillaDll = Path.Combine(_patchFilesPath, "Assembly-CSharp.dll");
            var patchedDll = Path.Combine(_patchFilesPath, "Assembly-CSharp_patched.dll");
            var lobotomypatchDll = Path.Combine(_patchFilesPath, "Lobotomypatch.dll");
            var harmonyDll = Path.Combine(_patchFilesPath, "0Harmony.dll");
            _ = _mockFileSystem.Setup(f => f.GetFiles(_patchFilesPath, "*.dll"))
                .Returns(new[] { vanillaDll, patchedDll, lobotomypatchDll, harmonyDll });

            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert - only 0Harmony.dll should be copied via the DLL loop
            _mockFileSystem.Verify(f => f.CopyFile(vanillaDll, It.IsAny<string>(), true), Times.Never);
            _mockFileSystem.Verify(f => f.CopyFile(lobotomypatchDll, It.IsAny<string>(), true), Times.Never);
            _mockFileSystem.Verify(f => f.CopyFile(harmonyDll, Path.Combine(_gameManagedPath, "0Harmony.dll"), true), Times.Once);
        }

        [Fact]
        public void Install_copies_BaseMod_directory_to_Managed_when_it_exists()
        {
            // Arrange
            var baseModSource = Path.Combine(_patchFilesPath, "BaseMod");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(baseModSource)).Returns(true);

            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            var baseModDest = Path.Combine(_gameManagedPath, "BaseMod");
            _mockFileSystem.Verify(f => f.CopyDirectory(baseModSource, baseModDest, true), Times.Once);
        }

        [Fact]
        public void Install_skips_BaseMod_copy_when_directory_does_not_exist()
        {
            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            var baseModDest = Path.Combine(_gameManagedPath, "BaseMod");
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), baseModDest, true), Times.Never);
        }

        [Fact]
        public void Install_creates_BaseModList_v2_xml_when_it_does_not_exist()
        {
            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            var baseModListPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods", "BaseModList_v2.xml");
            _mockFileSystem.Verify(f => f.WriteAllText(baseModListPath, It.Is<string>(s => s.Contains("<ModListXml"))), Times.Once);
        }

        [Fact]
        public void Install_creates_BaseMods_directory_when_it_does_not_exist()
        {
            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            var baseModsPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            _mockFileSystem.Verify(f => f.CreateDirectory(baseModsPath), Times.Once);
        }

        [Fact]
        public void Install_skips_BaseModList_v2_xml_when_it_already_exists()
        {
            // Arrange
            var baseModsPath = Path.Combine(_gamePath, "LobotomyCorp_Data", "BaseMods");
            var baseModListPath = Path.Combine(baseModsPath, "BaseModList_v2.xml");
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(baseModsPath)).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(baseModListPath)).Returns(true);

            // Act
            _lmmInstaller.Install(_gamePath, _lmmSourcePath);

            // Assert
            _mockFileSystem.Verify(f => f.WriteAllText(baseModListPath, It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Install_throws_when_PatchFiles_directory_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_patchFilesPath)).Returns(false);

            // Act & Assert
            _ = Assert.Throws<DirectoryNotFoundException>(() => _lmmInstaller.Install(_gamePath, _lmmSourcePath));
        }

        [Fact]
        public void Install_throws_when_patched_Assembly_CSharp_missing()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_patchFilesPath, "Assembly-CSharp_patched.dll"))).Returns(false);

            // Act & Assert
            _ = Assert.Throws<FileNotFoundException>(() => _lmmInstaller.Install(_gamePath, _lmmSourcePath));
        }

        [Fact]
        public void Install_throws_when_gamePath_is_null()
        {
            // Act & Assert
            _ = Assert.Throws<System.ArgumentNullException>(() => _lmmInstaller.Install(null!, _lmmSourcePath));
        }
    }
}
