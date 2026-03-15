// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Commands
{
    public sealed class DeployCommandTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IConfigManager> _mockConfigManager;
        private readonly Mock<IProcessRunner> _mockProcessRunner;
        private readonly DeployCommand _deployCommand;
        private readonly string _gamePath = "/test/game/path";
        private readonly string _repoRoot = "/test/repo/root";

        public DeployCommandTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockConfigManager = new Mock<IConfigManager>();
            _mockProcessRunner = new Mock<IProcessRunner>();
            _deployCommand = new DeployCommand(_mockConfigManager.Object, _mockFileSystem.Object, _mockProcessRunner.Object);

            // Setup default config
            Config config = new()
            {
                GamePath = _gamePath
            };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetCurrentDirectory()).Returns(_repoRoot);
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_repoRoot, "LobotomyCorporationMods.sln"))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFileSize(It.IsAny<string>())).Returns(100);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns([]);
        }

        [Fact]
        public void Run_Deployment_CopiesAllRequiredFiles()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "LobotomyCorporationMods.Common.*.dll"))
                .Returns(["LobotomyCorporationMods.Common.6.0.2.dll"]);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);

            // Verify tool project DLL deployments
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyPlaywright.Plugin.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("RetargetHarmony.dll")), It.IsAny<string>(), true), Times.Once);

            // Verify mod DLL deployments
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.BadLuckProtectionForGifts.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.BugFixes.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.DebugPanel.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.FreeCustomization.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.GiftAlertIcon.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.NotifyWhenAgentReceivesGift.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.WarnWhenAgentWillDieFromWorking.dll")), It.IsAny<string>(), true), Times.Once);

            // Verify Common DLL deployed for each mod (7 mods)
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyCorporationMods.Common")), It.IsAny<string>(), true), Times.Exactly(7));

            // Verify interop DLLs
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony109.dll")), It.IsAny<string>(), true), Times.Once);
            // 0Harmony12.dll is copied twice: once for itself, once as source for 12Harmony.dll
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony12.dll")), It.IsAny<string>(), true), Times.Exactly(2));
        }

        [Fact]
        public void Run_BuildPhase_HandlesBuildFailures()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(It.Is<string>(s => s.EndsWith("LobotomyCorporationMods.sln")))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(1); // Failure

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_Deployment_DeploysModContentDirectories()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), "LobotomyCorporationMods.Common.*.dll"))
                .Returns(["LobotomyCorporationMods.Common.6.0.2.dll"]);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);

            // Verify CopyDirectory called for content dirs (Info, Assets, Localize exist for all 7 mods since DirectoryExists returns true)
            _mockFileSystem.Verify(f => f.CopyDirectory(It.IsAny<string>(), It.IsAny<string>(), true), Times.Exactly(21));
        }
    }
}
