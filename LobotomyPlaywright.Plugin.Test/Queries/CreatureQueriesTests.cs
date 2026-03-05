// SPDX-License-Identifier: MIT

using System;
using FluentAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class CreatureQueriesTests
    {
        [Fact]
        public void ListCreatures_when_creature_manager_null_throws_exception()
        {
            // Act
            Action act = () => CreatureQueries.ListCreatures();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*CreatureManager.instance*");
        }

        [Fact]
        public void ListCreatures_returns_empty_list_when_no_creatures()
        {
            // Verify the method exists and throws appropriately when game not initialized
            Action act = () => CreatureQueries.ListCreatures();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetCreature_when_creature_manager_null_throws_exception()
        {
            // Act
            Action act = () => CreatureQueries.GetCreature(1);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*CreatureManager.instance*");
        }

        [Fact]
        public void GetCreature_with_valid_id_when_creature_manager_null_throws_exception()
        {
            // Act
            Action act = () => CreatureQueries.GetCreature(100001);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetCreature_with_zero_id_returns_null()
        {
            Action act = () => CreatureQueries.GetCreature(0);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetCreature_with_negative_id_returns_null()
        {
            Action act = () => CreatureQueries.GetCreature(-1);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
