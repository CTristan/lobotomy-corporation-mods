// SPDX-License-Identifier: MIT

#nullable enable
#pragma warning disable CA1515 // Test classes must be public for xUnit

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;
using Moq;
using Xunit;
using Config = LobotomyPlaywright.Interfaces.Configuration.Config;

namespace LobotomyPlaywright.Tests.Commands;

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

        _mockTcpClientFactory.Setup(f => f()).Returns(_mockTcpClient.Object);

        var config = new Config
        {
            GamePath = "/test/game/path",
            CrossoverBottle = "TestBottle",
            TcpPort = 8484,
            LaunchTimeoutSeconds = 120,
            ShutdownTimeoutSeconds = 10
        };
        _mockConfigManager.Setup(c => c.Load()).Returns(config);

        _screenshotCommand = new ScreenshotCommand(_mockConfigManager.Object, _mockTcpClientFactory.Object);
    }

    [Fact]
    public void Run_WhenConfigMissing_ReturnsError()
    {
        // Arrange
        _mockConfigManager.Setup(c => c.Load()).Throws(new FileNotFoundException("Config not found"));

        // Act
        var result = _screenshotCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void Run_WhenInvalidFormat_ReturnsError()
    {
        // Arrange
        var args = new[] { "--format", "invalid" };

        // Act
        var result = _screenshotCommand.Run(args);

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void Run_WhenInvalidDisplayFormat_ReturnsError()
    {
        // Arrange
        var args = new[] { "--display", "invalid" };

        // Act
        var result = _screenshotCommand.Run(args);

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void Run_WhenConnectionFails_ReturnsError()
    {
        // Arrange
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new System.Net.Sockets.SocketException());

        // Act
        var result = _screenshotCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void Run_WhenCommandSucceeds_ReturnsSuccess()
    {
        // Arrange
        var responseData = new Dictionary<string, object>
        {
            { "filename", "screenshot_test.png" },
            { "path", "/test/path/screenshot_test.png" },
            { "size", 1234567 },
            { "timestamp", "2026-03-06T18:00:00Z" },
            { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." },
            { "mimeType", "image/png" }
        };
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Returns(responseData);

        // Act
        var result = _screenshotCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.Connect("localhost", 8484), Times.Once);
        _mockTcpClient.Verify(c => c.SendCommandWithData("screenshot",
            It.Is<Dictionary<string, object>>(p =>
                p.ContainsKey("format") && p["format"].ToString() == "base64")), Times.Once);
    }

    [Fact]
    public void Run_WithFormatPath_SendsPathFormat()
    {
        // Arrange
        var responseData = new Dictionary<string, object>
        {
            { "filename", "screenshot_test.png" },
            { "path", "/test/path/screenshot_test.png" },
            { "size", 1234567 },
            { "timestamp", "2026-03-06T18:00:00Z" }
        };
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Returns(responseData);

        var args = new[] { "--format", "path" };

        // Act
        var result = _screenshotCommand.Run(args);

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData("screenshot",
            It.Is<Dictionary<string, object>>(p =>
                p.ContainsKey("format") && p["format"].ToString() == "path")), Times.Once);
    }

    [Fact]
    public void Run_WithDisplayJson_UsesJsonDisplay()
    {
        // Arrange
        var responseData = new Dictionary<string, object>
        {
            { "filename", "screenshot_test.png" },
            { "path", "/test/path/screenshot_test.png" },
            { "size", 1234567 },
            { "timestamp", "2026-03-06T18:00:00Z" },
            { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." }
        };
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Returns(responseData);

        var args = new[] { "--display", "json" };

        // Act
        var result = _screenshotCommand.Run(args);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Run_WithCustomHostAndPort_UsesCustomConnection()
    {
        // Arrange
        var responseData = new Dictionary<string, object>
        {
            { "filename", "screenshot_test.png" },
            { "path", "/test/path/screenshot_test.png" },
            { "size", 1234567 },
            { "timestamp", "2026-03-06T18:00:00Z" },
            { "base64", "iVBORw0KGgoAAAANSUhEUgAA..." }
        };
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Returns(responseData);

        var args = new[] { "--host", "192.168.1.100", "--port", "9000" };

        // Act
        var result = _screenshotCommand.Run(args);

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.Connect("192.168.1.100", 9000), Times.Once);
    }

    [Fact]
    public void Run_WhenResponseMissingFilename_ReturnsError()
    {
        // Arrange
        var responseData = new Dictionary<string, object>
        {
            { "path", "/test/path/screenshot_test.png" },
            { "size", 1234567 }
        };
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Returns(responseData);

        // Act
        var result = _screenshotCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public void Run_WhenCommandFails_ReturnsError()
    {
        // Arrange
        _mockTcpClient.Setup(c => c.SendCommandWithData("screenshot", It.IsAny<Dictionary<string, object>>()))
            .Throws(new InvalidOperationException("Command failed"));

        // Act
        var result = _screenshotCommand.Run(Array.Empty<string>());

        // Assert
        result.Should().NotBe(0);
    }
}
