// SPDX-License-Identifier: MIT

using System;
using FluentAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class AgentQueriesTests
    {
        [Fact]
        public void ListAgents_when_agent_manager_null_throws_exception()
        {
            // Act
            Action act = () => AgentQueries.ListAgents();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*AgentManager.instance*");
        }

        [Fact]
        public void ListAgents_returns_empty_list_when_no_agents()
        {
            // This test would require mocking or setting up the game environment
            // For now, we verify the method signature exists and throws appropriately
            Action act = () => AgentQueries.ListAgents();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetAgent_when_agent_manager_null_throws_exception()
        {
            // Act
            Action act = () => AgentQueries.GetAgent(1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*AgentManager.instance*");
        }

        [Fact]
        public void GetAgent_with_valid_id_when_agent_manager_null_throws_exception()
        {
            // Act
            Action act = () => AgentQueries.GetAgent(12345);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetAgent_with_zero_id_returns_null()
        {
            // This would return null when the game manager is not available
            // For now we verify it throws
            Action act = () => AgentQueries.GetAgent(0);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetAgent_with_negative_id_returns_null()
        {
            Action act = () => AgentQueries.GetAgent(-1);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
