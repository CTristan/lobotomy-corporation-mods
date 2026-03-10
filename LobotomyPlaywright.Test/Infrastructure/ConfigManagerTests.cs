// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Tests.Infrastructure
{
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
            var json = /*lang=json,strict*/ @"{
  ""gamePath"": ""/path/to/game"",
  ""crossoverBottle"": ""TestBottle"",
  ""tcpPort"": 8484,
  ""launchTimeoutSeconds"": 120,
  ""shutdownTimeoutSeconds"": 10
}";
            _ = _mockFileSystem.Setup(f => f.FileExists("/test/config.json")).Returns(true);
            _ = _mockFileSystem.Setup(f => f.ReadAllText("/test/config.json")).Returns(json);

            // Act
            Config config = _configManager.Load();

            // Assert
            _ = config.GamePath.Should().Be("/path/to/game");
            _ = config.CrossoverBottle.Should().Be("TestBottle");
            _ = config.TcpPort.Should().Be(8484);
            _ = config.LaunchTimeoutSeconds.Should().Be(120);
            _ = config.ShutdownTimeoutSeconds.Should().Be(10);
        }

        [Fact]
        public void Load_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            _ = _mockFileSystem.Setup(f => f.FileExists("/test/config.json")).Returns(false);

            // Act
            Action action = () => _configManager.Load();

            // Assert
            _ = action.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void Save_WritesConfigToFile()
        {
            // Arrange
            Config config = new()
            {
                GamePath = "/test/path",
                CrossoverBottle = "TestBottle",
                TcpPort = 9000,
                LaunchTimeoutSeconds = 60,
                ShutdownTimeoutSeconds = 5
            };

            string? writtenContent = null;
            _ = _mockFileSystem
                .Setup(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, content) => writtenContent = content);
            _ = _mockFileSystem.Setup(f => f.DirectoryExists("/test")).Returns(false);
            _ = _mockFileSystem.Setup(f => f.CreateDirectory("/test"));

            // Act
            _configManager.Save(config);

            // Assert
            _ = writtenContent.Should().NotBeNull();
            _ = writtenContent.Should().Contain("/test/path");
            _ = writtenContent.Should().Contain("TestBottle");
            _ = writtenContent.Should().Contain("9000");
            _mockFileSystem.Verify(f => f.CreateDirectory("/test"), Times.Once);
        }
    }
}
