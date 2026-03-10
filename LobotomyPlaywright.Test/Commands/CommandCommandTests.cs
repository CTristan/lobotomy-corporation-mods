// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;
using Moq;
using Xunit;

namespace LobotomyPlaywright.Test.Commands
{
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
            _ = Directory.CreateDirectory(tempDir);
            var configPath = Path.Combine(tempDir, "config.json");
            File.WriteAllText(configPath, /*lang=json,strict*/ @"{
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
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run([]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_WithHelpFlag_PrintsUsage()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["help"]);

            // Assert
            _ = result.Should().Be(0);
        }

        [Fact]
        public void Run_WithUnknownCommand_ReturnsError()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["unknown-command"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_PauseCommand_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "paused" } });

            // Act
            int result = command.Run(["pause"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()), Times.Once);
        }

        [Fact]
        public void Run_UnpauseCommand_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("unpause", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "unpaused" } });

            // Act
            int result = command.Run(["unpause"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData("unpause", It.IsAny<Dictionary<string, object>?>()), Times.Once);
        }

        [Fact]
        public void Run_FillEnergyCommand_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("fill-energy", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "energy_filled" }, { "energy", 100f } });

            // Act
            int result = command.Run(["fill-energy"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData("fill-energy", It.IsAny<Dictionary<string, object>?>()), Times.Once);
        }

        [Fact]
        public void Run_SetAgentStatsCommand_WithAgentId_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("set-agent-stats", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "stats_updated" }, { "agentId", 1L } });

            // Act
            int result = command.Run(["set-agent-stats", "--agent", "1", "--hp", "100", "--mental", "100"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("add-gift", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "gift_added" }, { "giftId", 123 } });

            // Act
            int result = command.Run(["add-gift", "--agent", "1", "--gift", "123"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("remove-gift", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "gift_removed" }, { "giftId", 123 } });

            // Act
            int result = command.Run(["remove-gift", "--agent", "1", "--gift", "123"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("set-qliphoth", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "qliphoth_set" } });

            // Act
            int result = command.Run(["set-qliphoth", "--creature", "100001", "--counter", "3"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("set-game-speed", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "speed_set" } });

            // Act
            int result = command.Run(["set-game-speed", "--speed", "2"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData(
                "set-game-speed",
                It.Is<Dictionary<string, object>>(d => (int)d["speed"] == 2)),
                Times.Once);
        }

        [Fact]
        public void Run_AssignWorkCommand_WithAllParams_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("assign-work", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "assigned" } });

            // Act
            int result = command.Run(["assign-work", "--agent", "1", "--creature", "100001", "--work", "instinct"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("deploy-agent", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "deployed" } });

            // Act
            int result = command.Run(["deploy-agent", "--agent", "1", "--sefira", "CHESED"]);

            // Assert
            _ = result.Should().Be(0);
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
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("recall-agent", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "recalled" } });

            // Act
            int result = command.Run(["recall-agent", "--agent", "1"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData(
                "recall-agent",
                It.Is<Dictionary<string, object>>(d => (long)d["agentId"] == 1)),
                Times.Once);
        }

        [Fact]
        public void Run_SuppressCommand_WithCreatureId_SendsCorrectRequest()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("suppress", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "suppressed" } });

            // Act
            int result = command.Run(["suppress", "--creature", "100001"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.SendCommandWithData(
                "suppress",
                It.Is<Dictionary<string, object>>(d => (long)d["creatureId"] == 100001)),
                Times.Once);
        }

        [Fact]
        public void Run_WithHostAndPortOverrides_SendsToCorrectDestination()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect("custom-host", 9999, It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "result", "paused" } });

            // Act
            int result = command.Run(["pause", "--host", "custom-host", "--port", "9999"]);

            // Assert
            _ = result.Should().Be(0);
            _mockTcpClient.Verify(c => c.Connect("custom-host", 9999, It.IsAny<double>()), Times.Once);
        }

        [Fact]
        public void Run_WhenServerReturnsError_DisplaysErrorMessage()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()));
            _ = _mockTcpClient.Setup(c => c.SendCommandWithData("pause", It.IsAny<Dictionary<string, object>?>()))
                .Returns(new Dictionary<string, object> { { "error", "Game not ready" } });

            // Act
            int result = command.Run(["pause"]);

            // Assert
            _ = result.Should().Be(0); // Command succeeds but displays error
        }

        [Fact]
        public void Run_WhenConnectionFails_ReturnsError()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);
            _ = _mockTcpClient.Setup(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()))
                .Throws(new InvalidOperationException("Connection refused"));

            // Act
            int result = command.Run(["pause"]);

            // Assert
            _ = result.Should().Be(1);
        }

        [Fact]
        public void Run_SetAgentStatsCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-agent-stats", "--hp", "100"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetAgentStatsCommand_WithInvalidAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-agent-stats", "--agent", "abc"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetAgentStatsCommand_WithInvalidHpValue_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-agent-stats", "--agent", "1", "--hp", "not-a-number"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_AddGiftCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["add-gift", "--gift", "123"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_AddGiftCommand_WithoutGiftId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["add-gift", "--agent", "1"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_RemoveGiftCommand_WithInvalidGiftId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["remove-gift", "--agent", "1", "--gift", "not-a-number"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetQliphothCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-qliphoth", "--counter", "3"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetQliphothCommand_WithoutCounter_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-qliphoth", "--creature", "100001"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetGameSpeedCommand_WithoutSpeed_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-game-speed"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetGameSpeedCommand_WithInvalidSpeed_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-game-speed", "--speed", "not-a-number"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetAgentInvincibleCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-agent-invincible", "--invincible", "true"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SetAgentInvincibleCommand_WithInvalidInvincibleValue_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["set-agent-invincible", "--agent", "1", "--invincible", "maybe"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_AssignWorkCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["assign-work", "--creature", "100001", "--work", "instinct"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_AssignWorkCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["assign-work", "--agent", "1", "--work", "instinct"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_AssignWorkCommand_WithoutWorkType_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["assign-work", "--agent", "1", "--creature", "100001"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_DeployAgentCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["deploy-agent", "--sefira", "CHESED"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_DeployAgentCommand_WithoutSefira_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["deploy-agent", "--agent", "1"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_RecallAgentCommand_WithoutAgentId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["recall-agent"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_SuppressCommand_WithoutCreatureId_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["suppress"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public void Run_WithInvalidPort_ReturnsErrorWithoutConnecting()
        {
            // Arrange
            CommandCommand command = new(_configManager, _tcpClientFactory);

            // Act
            int result = command.Run(["pause", "--port", "not-a-number"]);

            // Assert
            _ = result.Should().Be(1);
            _mockTcpClient.Verify(c => c.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<double>()), Times.Never);
        }
    }
}
