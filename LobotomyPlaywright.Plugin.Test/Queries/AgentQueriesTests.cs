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
        public void ListAgents_returns_empty_list_when_no_game_state()
        {
            // Act - AgentManager.instance creates a new instance if null
            // with empty agentList, so we should get an empty list
            var agents = AgentQueries.ListAgents();

            // Assert
            agents.Should().NotBeNull();
            agents.Should().BeEmpty();
        }

        [Fact]
        public void ListAgents_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => AgentQueries.ListAgents();

            // Assert - method should not throw
            act.Should().NotThrow();
        }

        [Fact]
        public void GetAgent_with_nonexistent_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(999999);

            // Assert
            agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_with_zero_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(0);

            // Assert
            agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_with_negative_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(-1);

            // Assert
            agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => AgentQueries.GetAgent(12345);

            // Assert - method should not throw
            act.Should().NotThrow();
        }
    }
}
