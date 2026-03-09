// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
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
            List<AgentData> agents = AgentQueries.ListAgents();

            // Assert
            _ = agents.Should().NotBeNull();
            _ = agents.Should().BeEmpty();
        }

        [Fact]
        public void ListAgents_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => AgentQueries.ListAgents();

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }

        [Fact]
        public void GetAgent_with_nonexistent_id_returns_null()
        {
            // Act
            AgentData agent = AgentQueries.GetAgent(999999);

            // Assert
            _ = agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_with_zero_id_returns_null()
        {
            // Act
            AgentData agent = AgentQueries.GetAgent(0);

            // Assert
            _ = agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_with_negative_id_returns_null()
        {
            // Act
            AgentData agent = AgentQueries.GetAgent(-1);

            // Assert
            _ = agent.Should().BeNull();
        }

        [Fact]
        public void GetAgent_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => AgentQueries.GetAgent(12345);

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }
    }
}
