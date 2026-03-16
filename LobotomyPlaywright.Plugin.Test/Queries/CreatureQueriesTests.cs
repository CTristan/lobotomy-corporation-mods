// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Queries;
using Xunit;

namespace LobotomyCorporationMods.Playwright.Test.Queries
{
    /// <summary>
    /// Tests for CreatureQueries.
    /// </summary>
    public class CreatureQueriesTests
    {
        /// <summary>
        /// Tests ListCreatures returns empty list when no game state.
        /// </summary>
        [Fact]
        public void ListCreatures_returns_empty_list_when_no_game_state()
        {
            // Act - CreatureManager.instance creates a new instance if null
            // with empty list, so we should get an empty list
            var creatures = CreatureQueries.ListCreatures();

            // Assert
            _ = creatures.Should().NotBeNull();
            _ = creatures.Should().BeEmpty();
        }

        /// <summary>
        /// Tests ListCreatures method exists and is callable.
        /// </summary>
        [Fact]
        public void ListCreatures_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => CreatureQueries.ListCreatures();

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }

        /// <summary>
        /// Tests GetCreature with nonexistent id returns null.
        /// </summary>
        [Fact]
        public void GetCreature_with_nonexistent_id_returns_null()
        {
            // Act
            var creature = CreatureQueries.GetCreature(999999);

            // Assert
            _ = creature.Should().BeNull();
        }

        /// <summary>
        /// Tests GetCreature with zero id returns null.
        /// </summary>
        [Fact]
        public void GetCreature_with_zero_id_returns_null()
        {
            // Act
            var creature = CreatureQueries.GetCreature(0);

            // Assert
            _ = creature.Should().BeNull();
        }

        /// <summary>
        /// Tests GetCreature method exists and is callable.
        /// </summary>
        [Fact]
        public void GetCreature_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => CreatureQueries.GetCreature(12345);

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }
    }
}
