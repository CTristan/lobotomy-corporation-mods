// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class GameStateQueriesTests
    {
        [Fact]
        public void IsGameQueryable_when_game_managers_not_available_returns_false()
        {
            // Act
            bool result = GameStateQueries.IsGameQueryable();

            // Assert
            _ = result.Should().BeFalse();
        }

        [Fact]
        public void GetStatus_when_game_manager_null_throws_exception()
        {
            // Act
            Action act = () => GameStateQueries.GetStatus();

            // Assert
            _ = act.Should().Throw<InvalidOperationException>()
                .WithMessage("*GameManager.currentGameManager*");
        }
    }
}
