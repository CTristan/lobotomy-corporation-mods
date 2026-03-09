// SPDX-License-Identifier: MIT

using AwesomeAssertions;
using LobotomyPlaywright.Commands;
using LobotomyPlaywright.Protocol;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Commands;

public class DebugCommandsTests
{
    [Fact]
    public void HandleSetAgentStats_null_params_returns_error()
    {
        // Arrange
        var request = new Request { id = "req-1", action = "set-agent-stats", Params = null };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
        response.error.Should().Contain("Invalid parameters");
    }

    [Fact]
    public void HandleSetAgentStats_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "set-agent-stats",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "hp", 100f } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
        response.error.Should().Contain("Invalid agentId");
    }

    [Fact]
    public void HandleAddGift_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "add-gift",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "giftId", 123 } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
        response.error.Should().Contain("Invalid agentId");
    }

    [Fact]
    public void HandleAddGift_missing_giftId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "add-gift",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "agentId", 1L } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
        response.error.Should().Contain("Invalid");
    }

    [Fact]
    public void HandleRemoveGift_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "remove-gift",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "giftId", 123 } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleSetQliphoth_missing_creatureId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "set-qliphoth",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "counter", 3 } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
        response.error.Should().Contain("Invalid creatureId");
    }

    [Fact]
    public void HandleSetGameSpeed_missing_speed_returns_error()
    {
        // Arrange
        var request = new Request { id = "req-1", action = "set-game-speed", Params = new System.Collections.Generic.Dictionary<string, object>() };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleSetAgentInvincible_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "set-agent-invincible",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "invincible", true } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }
}

public class PlayerActionCommandsTests
{
    [Fact]
    public void HandleAssignWork_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
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
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleAssignWork_missing_creatureId_returns_error()
    {
        // Arrange
        var request = new Request
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
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleAssignWork_missing_workType_returns_error()
    {
        // Arrange
        var request = new Request
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
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleDeployAgent_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "deploy-agent",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "sefira", "CHESED" } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleDeployAgent_missing_sefira_returns_error()
    {
        // Arrange
        var request = new Request
        {
            id = "req-1",
            action = "deploy-agent",
            Params = new System.Collections.Generic.Dictionary<string, object> { { "agentId", 1L } }
        };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleRecallAgent_missing_agentId_returns_error()
    {
        // Arrange
        var request = new Request { id = "req-1", action = "recall-agent", Params = new System.Collections.Generic.Dictionary<string, object>() };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }

    [Fact]
    public void HandleSuppress_missing_creatureId_returns_error()
    {
        // Arrange
        var request = new Request { id = "req-1", action = "suppress", Params = new System.Collections.Generic.Dictionary<string, object>() };

        // Act
        var response = CommandRouter.HandleCommand(request);

        // Assert
        response.Should().NotBeNull();
        response.status.Should().Be("error");
    }
}
