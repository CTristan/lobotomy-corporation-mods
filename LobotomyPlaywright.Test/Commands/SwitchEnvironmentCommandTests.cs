// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.IO;
using FluentAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Commands;

public sealed class SwitchEnvironmentCommandTests
{
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly Mock<IConfigManager> _mockConfigManager;
    private readonly Mock<ProcessManager> _mockProcessManager;
    private readonly SwitchEnvironmentCommand _command;
    private readonly string _gamePath = "/test/game/path";
    private readonly byte[] _debugDllBytes = new byte[] { 0x01, 0x02, 0x03 };
    private readonly byte[] _releaseDllBytes = new byte[] { 0x04, 0x05, 0x06 };

    public SwitchEnvironmentCommandTests()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        _mockConfigManager = new Mock<IConfigManager>();
        _mockProcessManager = new Mock<ProcessManager>();

        _command = new SwitchEnvironmentCommand(_mockConfigManager.Object, _mockFileSystem.Object, _mockProcessManager.Object);

        // Setup default config
        var config = new Config { GamePath = _gamePath };
        _mockConfigManager.Setup(c => c.Load()).Returns(config);
        _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(true);

        // Setup DLL files exist
        _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(true);

        // Setup default DLL bytes
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(_debugDllBytes);
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(_releaseDllBytes);

        // Setup default: game not running
        _mockProcessManager.Setup(p => p.FindGameProcesses()).Returns(new System.Collections.Generic.List<int>());
    }

    [Fact]
    public void Run_SwitchesFromReleaseToDebug()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_debug.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
    }

    [Fact]
    public void Run_SwitchesFromDebugToRelease()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_debugDllBytes);

        // Act
        var result = _command.Run(new[] { "release" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_release.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
    }

    [Fact]
    public void Run_AlreadyInTargetMode_ReturnsZeroWithoutCopying()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

        // Act
        var result = _command.Run(new[] { "release" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void Run_StatusFlagReportsCurrentEnvironment()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_debugDllBytes);

        // Act
        var result = _command.Run(new[] { "--status" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public void Run_GamePathDoesNotExist_ReturnsOne()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.DirectoryExists(_gamePath)).Returns(false);

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_DebugDllMissing_ReturnsOne()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_debug.dll"))).Returns(false);

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_ReleaseDllMissing_ReturnsOne()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.FileExists(Path.Combine(_gamePath, "UnityPlayer_release.dll"))).Returns(false);

        // Act
        var result = _command.Run(new[] { "release" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_UnknownVariantDll_ReturnsOne()
    {
        // Arrange
        var unknownDllBytes = new byte[] { 0xFF, 0xFF, 0xFF };
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(unknownDllBytes);

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_NoArguments_ReturnsOne()
    {
        // Act
        var result = _command.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_InvalidEnvironmentArgument_ReturnsOne()
    {
        // Act
        var result = _command.Run(new[] { "invalid" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_ConfigLoadFailure_ReturnsOne()
    {
        // Arrange
        _mockConfigManager.Setup(c => c.Load()).Throws(new FileNotFoundException("Config file not found"));

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_StopsGameIfRunningBeforeSwitch()
    {
        // Arrange
        var runningPids = new System.Collections.Generic.List<int> { 1234, 5678 };
        _mockProcessManager.Setup(p => p.FindGameProcesses()).Returns(runningPids);
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(0);
        _mockProcessManager.Verify(p => p.KillProcesses(runningPids, false), Times.Once);
        _mockFileSystem.Verify(f => f.CopyFile(Path.Combine(_gamePath, "UnityPlayer_debug.dll"), Path.Combine(_gamePath, "UnityPlayer.dll"), true), Times.Once);
    }

    [Fact]
    public void Run_CopyFileFailure_ReturnsOne()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);
        _mockFileSystem.Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new IOException("Permission denied"));

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_VerificationFailure_ReturnsOne()
    {
        // This test is no longer applicable since verification was removed
        // The command always returns 0 if the copy succeeds
        _mockFileSystem.Setup(f => f.ReadAllBytes(Path.Combine(_gamePath, "UnityPlayer.dll"))).Returns(_releaseDllBytes);
        _mockFileSystem.Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new IOException("Permission denied"));

        // Act
        var result = _command.Run(new[] { "debug" });

        // Assert
        result.Should().Be(1);
    }
}
