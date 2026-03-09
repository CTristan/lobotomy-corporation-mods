// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using Moq;
using Xunit;
using Config = LobotomyPlaywright.Interfaces.Configuration.Config;

namespace LobotomyPlaywright.Tests.Commands
{
    public sealed class ScreenshotCommandTests
    {
        private readonly Mock<ITcpClient> _mockTcpClient;
        private readonly Mock<Func<ITcpClient>> _mockTcpClientFactory;
        private readonly Mock<IConfigManager> _mockConfigManager;
        private readonly ScreenshotCommand _screenshotCommand;

        public ScreenshotCommandTests()
        {
            _mockTcpClient = new Mock<ITcpClient>();
            _mockTcpClientFactory = new Mock<Func<ITcpClient>>();
            _mockConfigManager = new Mock<IConfigManager>();

            _ = _mockTcpClientFactory.Setup(f => f()).Returns(_mockTcpClient.Object);

            Config config = new()
            {
                GamePath = "/test/game/path",
                CrossoverBottle = "TestBottle",
                TcpPort = 8484,
                LaunchTimeoutSeconds = 120,
                ShutdownTimeoutSeconds = 10
            };
            _ = _mockConfigManager.Setup(c => c.Load()).Returns(config);

            _screenshotCommand = new ScreenshotCommand(_mockConfigManager.Object, _mockTcpClientFactory.Object);
        }

        [Fact]
        public void Run_WhenConfigMissing_ReturnsError()
        {
            // Arrange
            _ = _mockConfigManager.Setup(c => c.Load()).Throws(new FileNotFoundException("Config not found"));

            // Act
            int result = _screenshotCommand.Run([]);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_WhenInvalidFormat_ReturnsError()
        {
            // Arrange
            string[] args = ["--format", "invalid"];

            // Act
            int result = _screenshotCommand.Run(args);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_WhenInvalidDisplayFormat_ReturnsError()
        {
            // Arrange
            string[] args = ["--display", "invalid"];

            // Act
            int result = _screenshotCommand.Run(args);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_WhenConnectionFails_ReturnsError()
        {
            // Arrange
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new System.Net.Sockets.SocketException());

            // Act
            int result = _screenshotCommand.Run([]);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_WhenCommandSucceeds_ReturnsSuccess()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "filename", "screenshot_test.png" },
                { "path", "/test/path/screenshot_test.png" },
                { "size", 1234567 },
                { "timestamp", "2026-03-06T18:00:00Z" },
                { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." },
                { "mimeType", "image/png" }
            };
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _screenshotCommand.Run([]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Connect("localhost", 8484), Times.Once);
            _mockTcpClient.Verify(c => c.SendCommandWithData("screenshot",
                It.Is<Dictionary<string, object>>(p =>
                    p.ContainsKey("format") && p["format"].ToString() == "base64")), Times.Once);
        }

        [Fact]
        public void Run_WithFormatPath_SendsPathFormat()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "filename", "screenshot_test.png" },
                { "path", "/test/path/screenshot_test.png" },
                { "size", 1234567 },
                { "timestamp", "2026-03-06T18:00:00Z" }
            };
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            string[] args = ["--format", "path"];

            // Act
            int result = _screenshotCommand.Run(args);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData("screenshot",
                It.Is<Dictionary<string, object>>(p =>
                    p.ContainsKey("format") && p["format"].ToString() == "path")), Times.Once);
        }

        [Fact]
        public void Run_WithDisplayJson_UsesJsonDisplay()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "filename", "screenshot_test.png" },
                { "path", "/test/path/screenshot_test.png" },
                { "size", 1234567 },
                { "timestamp", "2026-03-06T18:00:00Z" },
                { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." }
            };
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            string[] args = ["--display", "json"];

            // Act
            int result = _screenshotCommand.Run(args);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_WithCustomHostAndPort_UsesCustomConnection()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "filename", "screenshot_test.png" },
                { "path", "/test/path/screenshot_test.png" },
                { "size", 1234567 },
                { "timestamp", "2026-03-06T18:00:00Z" },
                { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." }
            };
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            string[] args = ["--host", "192.168.1.100", "--port", "9000"];

            // Act
            int result = _screenshotCommand.Run(args);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Connect("192.168.1.100", 9000), Times.Once);
        }

        [Fact]
        public void Run_WhenResponseMissingFilename_ReturnsError()
        {
            // Arrange
            Dictionary<string, object> responseData = new()
            {
                { "path", "/test/path/screenshot_test.png" },
                { "size", 1234567 }
            };
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Returns(responseData);

            // Act
            int result = _screenshotCommand.Run([]);

            // Assert
            _ = result.Should().NotBe(0);
        }

        [Fact]
        public void Run_WhenCommandFails_ReturnsError()
        {
            // Arrange
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
                .Throws(new InvalidOperationException("Command failed"));

            // Act
            int result = _screenshotCommand.Run([]);

            // Assert
            _ = result.Should().NotBe(0);
        }
    }
}
