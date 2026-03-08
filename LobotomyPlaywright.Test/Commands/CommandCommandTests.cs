// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Network;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Test.Commands;

public class CommandCommandTests
{
    private readonly Mock<ITcpClient> _mockTcpClient;
    private readonly IConfigManager _configManager;
    private readonly Func<ITcpClient> _tcpClientFactory;

    public CommandCommandTests()
    {
        _mockTcpClient = new Mock<ITcpClient>();
        _tcpClientFactory = () => _mockTcpClient.Object;

        // Create a temp config file for testing
        var tempDir = Path.Combine(Path.GetTempPath(), $"lobotomy_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var configPath = Path.Combine(tempDir, "config.json");
        File.WriteAllText(configPath, @"{
            ""gamePath"": ""/test/game"",
            ""crossoverBottle"": ""DXMT"",
            ""tcpPort"": 8484,
            ""launchTimeoutSeconds"": 60,
            ""shutdownTimeoutSeconds"": 10
        }");

        _configManager = new ConfigManager(new FileSystem(), configPath);
    }

    [Fact]
    public void Run_WithNoArguments_ReturnsError()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(Array.Empty<string>());

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_WithHelpFlag_PrintsUsage()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "help" });

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Run_WithUnknownCommand_ReturnsError()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "unknown-command" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_PauseCommand_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "paused" } });

        // Act
        var result = command.Run(new[] { "pause" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()), Times.Once);
    }

    [Fact]
    public void Run_UnpauseCommand_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("unpause", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "unpaused" } });

        // Act
        var result = command.Run(new[] { "unpause" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData("unpause", It.IsAny<Dictionary<string, object>?>()), Times.Once);
    }

    [Fact]
    public void Run_FillEnergyCommand_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("fill-energy", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "energy_filled" }, { "energy", 100f } });

        // Act
        var result = command.Run(new[] { "fill-energy" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData("fill-energy", It.IsAny<Dictionary<string, object>?>()), Times.Once);
    }

    [Fact]
    public void Run_SetAgentStatsCommand_WithAgentId_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("set-agent-stats", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "stats_updated" }, { "agentId", 1L } });

        // Act
        var result = command.Run(new[] { "set-agent-stats", "--agent", "1", "--hp", "100", "--mental", "100" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "set-agent-stats",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["agentId"] == 1 &&
                (float)d["hp"] == 100f &&
                (float)d["mental"] == 100f)),
            Times.Once);
    }

    [Fact]
    public void Run_AddGiftCommand_WithAgentAndGift_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("add-gift", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "gift_added" }, { "giftId", 123 } });

        // Act
        var result = command.Run(new[] { "add-gift", "--agent", "1", "--gift", "123" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "add-gift",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["agentId"] == 1 &&
                (int)d["giftId"] == 123)),
            Times.Once);
    }

    [Fact]
    public void Run_RemoveGiftCommand_WithAgentAndGift_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("remove-gift", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "gift_removed" }, { "giftId", 123 } });

        // Act
        var result = command.Run(new[] { "remove-gift", "--agent", "1", "--gift", "123" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "remove-gift",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["agentId"] == 1 &&
                (int)d["giftId"] == 123)),
            Times.Once);
    }

    [Fact]
    public void Run_SetQliphothCommand_WithCreatureAndCounter_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("set-qliphoth", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "qliphoth_set" } });

        // Act
        var result = command.Run(new[] { "set-qliphoth", "--creature", "100001", "--counter", "3" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "set-qliphoth",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["creatureId"] == 100001 &&
                (int)d["counter"] == 3)),
            Times.Once);
    }

    [Fact]
    public void Run_SetGameSpeedCommand_WithSpeed_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("set-game-speed", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "speed_set" } });

        // Act
        var result = command.Run(new[] { "set-game-speed", "--speed", "2" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "set-game-speed",
            It.Is<Dictionary<string, object>>(d => (int)d["speed"] == 2)),
            Times.Once);
    }

    [Fact]
    public void Run_AssignWorkCommand_WithAllParams_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("assign-work", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "assigned" } });

        // Act
        var result = command.Run(new[] { "assign-work", "--agent", "1", "--creature", "100001", "--work", "instinct" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "assign-work",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["agentId"] == 1 &&
                (long)d["creatureId"] == 100001 &&
                d["workType"].ToString() == "instinct")),
            Times.Once);
    }

    [Fact]
    public void Run_DeployAgentCommand_WithAllParams_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("deploy-agent", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "deployed" } });

        // Act
        var result = command.Run(new[] { "deploy-agent", "--agent", "1", "--sefira", "CHESED" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "deploy-agent",
            It.Is<Dictionary<string, object>>(d =>
                (long)d["agentId"] == 1 &&
                d["sefira"].ToString() == "CHESED")),
            Times.Once);
    }

    [Fact]
    public void Run_RecallAgentCommand_WithAgentId_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("recall-agent", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "recalled" } });

        // Act
        var result = command.Run(new[] { "recall-agent", "--agent", "1" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "recall-agent",
            It.Is<Dictionary<string, object>>(d => (long)d["agentId"] == 1)),
            Times.Once);
    }

    [Fact]
    public void Run_SuppressCommand_WithCreatureId_SendsCorrectRequest()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("suppress", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "suppressed" } });

        // Act
        var result = command.Run(new[] { "suppress", "--creature", "100001" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.SendCommandWithData(
            "suppress",
            It.Is<Dictionary<string, object>>(d => (long)d["creatureId"] == 100001)),
            Times.Once);
    }

    [Fact]
    public void Run_WithHostAndPortOverrides_SendsToCorrectDestination()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect("custom-host", 9999, It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "result", "paused" } });

        // Act
        var result = command.Run(new[] { "pause", "--host", "custom-host", "--port", "9999" });

        // Assert
        result.Should().Be(0);
        _mockTcpClient.Verify(c => c.Connect("custom-host", 9999, It.IsAny<double>()), Times.Once);
    }

    [Fact]
    public void Run_WhenServerReturnsError_DisplaysErrorMessage()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
        _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
            .Returns(new Dictionary<string, object> { { "error", "Game not ready" } });

        // Act
        var result = command.Run(new[] { "pause" });

        // Assert
        result.Should().Be(0); // Command succeeds but displays error
    }

    [Fact]
    public void Run_WhenConnectionFails_ReturnsError()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);
        _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()))
            .Throws(new InvalidOperationException("Connection refused"));

        // Act
        var result = command.Run(new[] { "pause" });

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Run_SetAgentStatsCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-agent-stats", "--hp", "100" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetAgentStatsCommand_WithInvalidAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-agent-stats", "--agent", "abc" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetAgentStatsCommand_WithInvalidHpValue_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-agent-stats", "--agent", "1", "--hp", "not-a-number" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_AddGiftCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "add-gift", "--gift", "123" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_AddGiftCommand_WithoutGiftId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "add-gift", "--agent", "1" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_RemoveGiftCommand_WithInvalidGiftId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "remove-gift", "--agent", "1", "--gift", "not-a-number" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetQliphothCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-qliphoth", "--counter", "3" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetQliphothCommand_WithoutCounter_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-qliphoth", "--creature", "100001" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetGameSpeedCommand_WithoutSpeed_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-game-speed" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetGameSpeedCommand_WithInvalidSpeed_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-game-speed", "--speed", "not-a-number" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetAgentInvincibleCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-agent-invincible", "--invincible", "true" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SetAgentInvincibleCommand_WithInvalidInvincibleValue_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "set-agent-invincible", "--agent", "1", "--invincible", "maybe" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_AssignWorkCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "assign-work", "--creature", "100001", "--work", "instinct" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_AssignWorkCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "assign-work", "--agent", "1", "--work", "instinct" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_AssignWorkCommand_WithoutWorkType_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "assign-work", "--agent", "1", "--creature", "100001" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_DeployAgentCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "deploy-agent", "--sefira", "CHESED" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_DeployAgentCommand_WithoutSefira_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "deploy-agent", "--agent", "1" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_RecallAgentCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "recall-agent" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_SuppressCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "suppress" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public void Run_WithInvalidPort_ReturnsErrorWithoutConnecting()
    {
        // Arrange
        var command = new CommandCommand(_configManager, _tcpClientFactory);

        // Act
        var result = command.Run(new[] { "pause", "--port", "not-a-number" });

        // Assert
        result.Should().Be(1);
        _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
    }
}
