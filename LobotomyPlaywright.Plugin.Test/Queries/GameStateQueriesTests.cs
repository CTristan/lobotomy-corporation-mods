// SPDX-License-Identifier: MIT

using System;
using AwesomeAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    /// <summary>
    /// Tests for GameStateQueries.
    /// </summary>
    public class GameStateQueriesTests
    {
        /// <summary>
        /// Tests IsGameQueryable when game managers not available returns false.
        /// </summary>
        [Fact]
        public void IsGameQueryable_when_game_managers_not_available_returns_false()
        {
            // Act
            var result = GameStateQueries.IsGameQueryable();

            // Assert
            _ = result.Should().BeFalse();
        }

        /// <summary>
        /// Tests GetStatus when game manager null throws exception.
        /// </summary>
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
