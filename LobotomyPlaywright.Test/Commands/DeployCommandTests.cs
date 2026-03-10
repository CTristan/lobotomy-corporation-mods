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
        }

        [Fact]
        public void Run_Deployment_CopiesAllRequiredFiles()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);

            _ = _mockProcessRunner.Setup(p => p.Run(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Func<string?, bool>>()))
                .Returns(0);

            // Act
            int result = _deployCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);

            // Verify file copy calls via IFileSystem
            // Note: 12Harmony.dll uses 0Harmony12.dll as source, so 0Harmony12.dll gets copied twice
            // Plugin DLLs
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("LobotomyPlaywright.Plugin.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("HarmonyDebugPanel.dll")), It.IsAny<string>(), true), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("RetargetHarmony.dll")), It.IsAny<string>(), true), Times.Once);

            // Interop DLLs
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony109.dll")), It.IsAny<string>(), true), Times.Once);
            // 0Harmony12.dll is copied twice: once from loop item "0Harmony12.dll", once from loop item "12Harmony.dll" (which uses 0Harmony12.dll as source)
            _mockFileSystem.Verify(f => f.CopyFile(It.Is<string>(s => s.Contains("0Harmony12.dll")), It.IsAny<string>(), true), Times.Exactly(2));
        }

        [Fact]
        public void Run_BuildPhase_HandlesBuildFailures()
        {
            // Arrange
            var repoRoot = Directory.GetCurrentDirectory();
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
    }
}
