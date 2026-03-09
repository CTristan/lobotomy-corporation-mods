// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.IO;
using FluentAssertions;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Infrastructure;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure;

public sealed class ConfigManagerTests
{
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly ConfigManager _configManager;

    public ConfigManagerTests()
    {
        _mockFileSystem = new Mock<IFileSystem>();
        _configManager = new ConfigManager(_mockFileSystem.Object, "/test/config.json");
    }

    [Fact]
    public void Load_WhenFileExists_ReturnsConfig()
    {
        // Arrange
        var json = @"{
  ""gamePath"": ""/path/to/game"",
  ""crossoverBottle"": ""TestBottle"",
  ""tcpPort"": 8484,
  ""launchTimeoutSeconds"": 120,
  ""shutdownTimeoutSeconds"": 10
}";
        _mockFileSystem.Setup(f => f.FileExists("/test/config.json")).Returns(true);
        _mockFileSystem.Setup(f => f.ReadAllText("/test/config.json")).Returns(json);

        // Act
        var config = _configManager.Load();

        // Assert
        config.GamePath.Should().Be("/path/to/game");
        config.CrossoverBottle.Should().Be("TestBottle");
        config.TcpPort.Should().Be(8484);
        config.LaunchTimeoutSeconds.Should().Be(120);
        config.ShutdownTimeoutSeconds.Should().Be(10);
    }

    [Fact]
    public void Load_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        _mockFileSystem.Setup(f => f.FileExists("/test/config.json")).Returns(false);

        // Act
        Action action = () => _configManager.Load();

        // Assert
        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Save_WritesConfigToFile()
    {
        // Arrange
        var config = new Config
        {
            GamePath = "/test/path",
            CrossoverBottle = "TestBottle",
            TcpPort = 9000,
            LaunchTimeoutSeconds = 60,
            ShutdownTimeoutSeconds = 5
        };

        string? writtenContent = null;
        _mockFileSystem
            .Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, content) => writtenContent = content);
        _mockFileSystem.Setup(f => f.DirectoryExists("/test")).Returns(false);
        _mockFileSystem.Setup(f => f.CreateDirectory("/test"));

        // Act
        _configManager.Save(config);

        // Assert
        writtenContent.Should().NotBeNull();
        writtenContent.Should().Contain("/test/path");
        writtenContent.Should().Contain("TestBottle");
        writtenContent.Should().Contain("9000");
        _mockFileSystem.Verify(f => f.CreateDirectory("/test"), Times.Once);
    }
}
