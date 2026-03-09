// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class CreatureQueriesTests
    {
        [Fact]
        public void ListCreatures_returns_empty_list_when_no_game_state()
        {
            // Act - CreatureManager.instance creates a new instance if null
            // with empty list, so we should get an empty list
            var creatures = CreatureQueries.ListCreatures();

            // Assert
            creatures.Should().NotBeNull();
            creatures.Should().BeEmpty();
        }

        [Fact]
        public void ListCreatures_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => CreatureQueries.ListCreatures();

            // Assert - method should not throw
            act.Should().NotThrow();
        }

        [Fact]
        public void GetCreature_with_nonexistent_id_returns_null()
        {
            // Act
            var creature = CreatureQueries.GetCreature(999999);

            // Assert
            creature.Should().BeNull();
        }

        [Fact]
        public void GetCreature_with_zero_id_returns_null()
        {
            // Act
            var creature = CreatureQueries.GetCreature(0);

            // Assert
            creature.Should().BeNull();
        }

        [Fact]
        public void GetCreature_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => CreatureQueries.GetCreature(12345);

            // Assert - method should not throw
            act.Should().NotThrow();
        }
    }
}
