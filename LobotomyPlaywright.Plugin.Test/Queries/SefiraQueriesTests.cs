// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class SefiraQueriesTests
    {
        [Fact]
        public void ListSefira_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => SefiraQueries.ListSefira();

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }

        [Fact]
        public void ListSefira_returns_data()
        {
            // Act - SefiraManager.instance creates a new instance if null
            // The SefiraManager always has data (the Sefira departments)
            var sefiras = SefiraQueries.ListSefira();

            // Assert - Should return data (even if empty agent/creature lists)
            _ = sefiras.Should().NotBeNull();
            // The number of sefiras is fixed in the game
            _ = sefiras.Should().HaveCountGreaterThanOrEqualTo(10); // At least the named Sefira
        }

        [Fact]
        public void GetSefira_with_valid_sefira_enum_returns_data()
        {
            // Act - Get a valid Sefira (MALKUT)
            var sefira = SefiraQueries.GetSefira(SefiraEnum.MALKUT);

            // Assert - Should return data
            _ = sefira.Should().NotBeNull();
            _ = sefira.AgentIds.Should().NotBeNull();
            _ = sefira.CreatureIds.Should().NotBeNull();
        }

        [Fact]
        public void GetSefira_method_exists_and_is_callable()
        {
            // Arrange & Act
            Action act = () => SefiraQueries.GetSefira(SefiraEnum.MALKUT);

            // Assert - method should not throw
            _ = act.Should().NotThrow();
        }
    }
}
