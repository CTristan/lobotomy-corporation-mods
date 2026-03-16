// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Queries;
using Xunit;

namespace LobotomyCorporationMods.Playwright.Test.Queries
{
    /// <summary>
    /// Tests for AgentQueries.
    /// </summary>
    public class AgentQueriesTests
    {
        /// <summary>
        /// Tests ListAgents returns empty list when no game state.
        /// </summary>
        [Fact]
        public void ListAgents_returns_empty_list_when_no_game_state()
        {
            // Act - AgentManager.instance creates a new instance if null
            // with empty agentList, so we should get an empty list
            var agents = AgentQueries.ListAgents();

            // Assert
            _ = agents.Should().NotBeNull();
            _ = agents.Should().BeEmpty();
        }

        /// <summary>
        /// Tests ListAgents method exists and is callable.
        /// </summary>
        [Fact]
        public void ListAgents_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => AgentQueries.ListAgents();

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }

        /// <summary>
        /// Tests GetAgent with nonexistent id returns null.
        /// </summary>
        [Fact]
        public void GetAgent_with_nonexistent_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(999999);

            // Assert
            _ = agent.Should().BeNull();
        }

        /// <summary>
        /// Tests GetAgent with zero id returns null.
        /// </summary>
        [Fact]
        public void GetAgent_with_zero_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(0);

            // Assert
            _ = agent.Should().BeNull();
        }

        /// <summary>
        /// Tests GetAgent with negative id returns null.
        /// </summary>
        [Fact]
        public void GetAgent_with_negative_id_returns_null()
        {
            // Act
            var agent = AgentQueries.GetAgent(-1);

            // Assert
            _ = agent.Should().BeNull();
        }

        /// <summary>
        /// Tests GetAgent method exists and is callable.
        /// </summary>
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
