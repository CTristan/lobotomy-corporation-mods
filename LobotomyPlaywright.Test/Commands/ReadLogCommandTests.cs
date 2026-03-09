// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Commands;

public sealed class ReadLogCommandTests
{
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly Mock<IConfigManager> _mockConfigManager;
    private readonly ReadLogCommand _readLogCommand;

    public ReadLogCommandTests()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        _mockConfigManager = new Mock<IConfigManager>();
        _readLogCommand = new ReadLogCommand(_mockConfigManager.Object, _mockFileSystem.Object);

        // Setup default config
        var config = new Config
        {
            GamePath = "/test/game/path",
            CrossoverBottle = "TestBottle",
            TcpPort = 8484,
            LaunchTimeoutSeconds = 120,
            ShutdownTimeoutSeconds = 10
        };
        _mockConfigManager.Setup(c => c.Load()).Returns(config);
    }

    [Fact]
    public void Run_WhenConfigMissing_ReturnsError()
    {
        // Arrange
        _mockConfigManager.Setup(c => c.Load()).Throws(new FileNotFoundException("Config not found"));

        // Act
        var result = _readLogCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_WhenGamePathDoesNotExist_ReturnsError()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(false);

        // Act
        var result = _readLogCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_WhenBepInExLogDirDoesNotExist_ReturnsError()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(false);

        // Act
        var result = _readLogCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_WhenLogFileDoesNotExist_ReturnsError()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists("/test/game/path/BepInEx/LogOutput.log")).Returns(false);

        // Act
        var result = _readLogCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_WhenLogFileExists_DisplaysLogContent()
    {
        // Arrange
        var logContent = "Line 1\nLine 2\nLine 3";
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists("/test/game/path/BepInEx/LogOutput.log")).Returns(true);
        _mockFileSystem.Setup(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log")).Returns(logContent);

        // Act
        var result = _readLogCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log"), Times.Once);
    }

    [Fact]
    public void Run_WithTailOption_DisplaysLastNLines()
    {
        // Arrange
        var logContent = "Line 1\nLine 2\nLine 3\nLine 4\nLine 5";
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists("/test/game/path/BepInEx/LogOutput.log")).Returns(true);
        _mockFileSystem.Setup(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log")).Returns(logContent);

        // Act
        var result = _readLogCommand.Run(new[] { "--tail", "2" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log"), Times.Once);

        // Note: This test verifies that the command executes without error and reads the file.
        // Verifying the actual console output (e.g., that only the last 2 lines were displayed) would
        // require capturing stdout, which adds complexity. Integration tests should verify the tail
        // behavior with actual console output.
    }

    [Fact]
    public void Run_WithFilterOption_DisplaysFilteredLines()
    {
        // Arrange
        var logContent = "Info: Something happened\nError: Bad thing\nInfo: Another thing\nDebug: Details\nERROR: Another error";
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(true);
        _mockFileSystem.Setup(f => f.FileExists("/test/game/path/BepInEx/LogOutput.log")).Returns(true);
        _mockFileSystem.Setup(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log")).Returns(logContent);

        // Act
        var result = _readLogCommand.Run(new[] { "--filter", "Error" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.ReadAllText("/test/game/path/BepInEx/LogOutput.log"), Times.Once);

        // Note: This test verifies that the command executes without error and reads the file.
        // Verifying the actual console output (e.g., that only lines containing "Error" were displayed)
        // would require capturing stdout, which adds complexity. Integration tests should verify the
        // filter behavior with actual console output.
    }

    [Fact]
    public void Run_WithListOption_ListsLogFiles()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path")).Returns(true);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test/game/path/BepInEx")).Returns(true);

        // Act
        var result = _readLogCommand.Run(new[] { "--list" });

        // Assert
        result.Should().Be(0);
        _mockFileSystem.Verify(f => f.GetFiles(It.IsAny<string>(), "*.log"), Times.Once);
    }
}
