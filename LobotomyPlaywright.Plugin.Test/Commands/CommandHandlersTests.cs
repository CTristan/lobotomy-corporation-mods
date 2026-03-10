// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.JsonModels;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Commands
{
    /// <summary>
    /// Tests for debug command handlers.
    /// </summary>
    public class DebugCommandsTests
    {
        /// <summary>
        /// Tests HandleSetAgentStats with null params returns error.
        /// </summary>
        [Fact]
        public void HandleSetAgentStats_null_params_returns_error()
        {
            // Arrange
            Request request = new() { id = "req-1", action = "set-agent-stats", Params = null };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Invalid parameters");
        }

        /// <summary>
        /// Tests HandleSetAgentStats with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleSetAgentStats_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "set-agent-stats",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "hp", 100f } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Invalid agentId");
        }

        /// <summary>
        /// Tests HandleAddGift with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleAddGift_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "add-gift",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "giftId", 123 } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Invalid agentId");
        }

        /// <summary>
        /// Tests HandleAddGift with missing giftId returns error.
        /// </summary>
        [Fact]
        public void HandleAddGift_missing_giftId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "add-gift",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "agentId", 1L } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Invalid");
        }

        /// <summary>
        /// Tests HandleRemoveGift with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleRemoveGift_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "remove-gift",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "giftId", 123 } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleSetQliphoth with missing creatureId returns error.
        /// </summary>
        [Fact]
        public void HandleSetQliphoth_missing_creatureId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "set-qliphoth",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "counter", 3 } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
            _ = response.error.Should().Contain("Invalid creatureId");
        }

        /// <summary>
        /// Tests HandleSetGameSpeed with missing speed returns error.
        /// </summary>
        [Fact]
        public void HandleSetGameSpeed_missing_speed_returns_error()
        {
            // Arrange
            Request request = new() { id = "req-1", action = "set-game-speed", Params = [] };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleSetAgentInvincible with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleSetAgentInvincible_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "set-agent-invincible",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "invincible", true } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }
    }

    /// <summary>
    /// Tests for player action command handlers.
    /// </summary>
    public class PlayerActionCommandsTests
    {
        /// <summary>
        /// Tests HandleAssignWork with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleAssignWork_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "assign-work",
                Params = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "creatureId", 100001L },
                    { "workType", "instinct" }
                }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleAssignWork with missing creatureId returns error.
        /// </summary>
        [Fact]
        public void HandleAssignWork_missing_creatureId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "assign-work",
                Params = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "agentId", 1L },
                    { "workType", "instinct" }
                }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleAssignWork with missing workType returns error.
        /// </summary>
        [Fact]
        public void HandleAssignWork_missing_workType_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "assign-work",
                Params = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "agentId", 1L },
                    { "creatureId", 100001L }
                }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleDeployAgent with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleDeployAgent_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "deploy-agent",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "sefira", "CHESED" } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleDeployAgent with missing sefira returns error.
        /// </summary>
        [Fact]
        public void HandleDeployAgent_missing_sefira_returns_error()
        {
            // Arrange
            Request request = new()
            {
                id = "req-1",
                action = "deploy-agent",
                Params = new System.Collections.Generic.Dictionary<string, object> { { "agentId", 1L } }
            };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleRecallAgent with missing agentId returns error.
        /// </summary>
        [Fact]
        public void HandleRecallAgent_missing_agentId_returns_error()
        {
            // Arrange
            Request request = new() { id = "req-1", action = "recall-agent", Params = [] };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }

        /// <summary>
        /// Tests HandleSuppress with missing creatureId returns error.
        /// </summary>
        [Fact]
        public void HandleSuppress_missing_creatureId_returns_error()
        {
            // Arrange
            Request request = new() { id = "req-1", action = "suppress", Params = [] };

            // Act
            var response = CommandRouter.HandleCommand(request);

            // Assert
            _ = response.Should().NotBeNull();
            _ = response.status.Should().Be("error");
        }
    }
}
