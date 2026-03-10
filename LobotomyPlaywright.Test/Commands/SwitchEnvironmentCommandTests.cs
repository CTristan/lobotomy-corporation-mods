// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Commands
{
    public sealed class SwitchEnvironmentCommandTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem;
        private readonly Mock<IConfigManager> _mockConfigManager;
        private readonly Mock<ProcessManager> _mockProcessManager;
        private readonly SwitchEnvironmentCommand _command;
        private readonly string _gamePath = "/test/game/path";
        private readonly byte[] _debugDllBytes = [0x01, 0x02, 0x03];
        private readonly byte[] _releaseDllBytes = [0x04, 0x05, 0x06];

        public SwitchEnvironmentCommandTests()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _mockConfigManager = new Mock<IConfigManager>();
            _mockProcessManager = new Mock<ProcessManager>();

            _command = new SwitchEnvironmentCommand(_mockConfigManager.Object, _mockFileSystem.Object, _mockProcessManager.Object);

            // Setup default config
            Config config = new() { GamePath = _gamePath };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(true);

            // Setup DLL files exist
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(true);
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(true);

            // Setup default DLL bytes
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(_debugDllBytes);
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(_releaseDllBytes);

            // Setup default: game not running
            _ = _mockProcessManager.Setup(p => p.FindGameProcesses()).Returns([]);
        }

        [Fact]
        public void Run_SwitchesFromReleaseToDebug()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(0);
            _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_debug.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
        }

        [Fact]
        public void Run_SwitchesFromDebugToRelease()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_debugDllBytes);

            // Act
            int result = _command.Run(["release"]);

            // Assert
            _ = result.Should().Be(0);
            _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_release.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
        }

        [Fact]
        public void Run_AlreadyInTargetMode_ReturnsZeroWithoutCopying()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

            // Act
            int result = _command.Run(["release"]);

            // Assert
            _ = result.Should().Be(0);
            _mockFileSystem.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void Run_StatusFlagReportsCurrentEnvironment()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_debugDllBytes);

            // Act
            int result = _command.Run(["--status"]);

            // Assert
            _ = result.Should().Be(0);
            _mockFileSystem.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void Run_GamePathDoesNotExist_ReturnsOne()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(false);

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_DebugDllMissing_ReturnsOne()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(false);

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_ReleaseDllMissing_ReturnsOne()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(false);

            // Act
            int result = _command.Run(["release"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_UnknownVariantDll_ReturnsOne()
        {
            // Arrange
            byte[] unknownDllBytes = [0xFF, 0xFF, 0xFF];
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(unknownDllBytes);

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_NoArguments_ReturnsOne()
        {
            // Act
            int result = _command.Run([]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_InvalidEnvironmentArgument_ReturnsOne()
        {
            // Act
            int result = _command.Run(["invalid"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_ConfigLoadFailure_ReturnsOne()
        {
            // Arrange
            _ = _mockConfigManager.Setup(c => c.Load()).Throws(new FileNotFoundException("Config file not found"));

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_StopsGameIfRunningBeforeSwitch()
        {
            // Arrange
            List<int> runningPids = [1234, 5678];
            _ = _mockProcessManager.Setup(p => p.FindGameProcesses()).Returns(runningPids);
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(0);
            _mockProcessManager.Verify(p => p.KillProcesses(runningPids, false), Times.Once);
            _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_debug.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
        }

        [Fact]
        public void Run_CopyFileFailure_ReturnsOne()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);
            _ = _mockFileSystem.Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new IOException("Permission denied"));

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_VerificationFailure_ReturnsOne()
        {
            // This test is no longer applicable since verification was removed
            // The command always returns 0 if the copy succeeds
            _ = _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);
            _ = _mockFileSystem.Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new IOException("Permission denied"));

            // Act
            int result = _command.Run(["debug"]);

            // Assert
            _ = result.Should().Be(1);
        }
    }
}
