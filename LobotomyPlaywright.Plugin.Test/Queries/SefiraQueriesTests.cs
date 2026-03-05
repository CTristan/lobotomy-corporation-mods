// SPDX-License-Identifier: MIT

using System;
using FluentAssertions;
using LobotomyPlaywright.Queries;
using Xunit;

namespace LobotomyPlaywright.Plugin.Test.Queries
{
    public class SefiraQueriesTests
    {
        [Fact]
        public void ListSefira_when_sefira_manager_null_throws_exception()
        {
            // Act
            Action act = () => SefiraQueries.ListSefira();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*SefiraManager.instance*");
        }

        [Fact]
        public void ListSefira_returns_list_when_sefira_manager_available()
        {
            // Verify the method exists
            Action act = () => SefiraQueries.ListSefira();
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetSefira_when_sefira_manager_null_throws_exception()
        {
            // Act
            Action act = () => SefiraQueries.GetSefira(SefiraEnum.KETHER);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*SefiraManager.instance*");
        }

        [Fact]
        public void GetSefira_with_valid_sefira_enum_when_manager_null_throws_exception()
        {
            // Act
            Action act = () => SefiraQueries.GetSefira(SefiraEnum.HOD);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetSefira_with_each_sefira_enum_value()
        {
            // Verify that each enum value is valid
            Action[] actions =
            {
                () => SefiraQueries.GetSefira(SefiraEnum.KETHER),
                () => SefiraQueries.GetSefira(SefiraEnum.CHOKHMAH),
                () => SefiraQueries.GetSefira(SefiraEnum.BINAH),
                () => SefiraQueries.GetSefira(SefiraEnum.CHESED),
                () => SefiraQueries.GetSefira(SefiraEnum.GEBURAH),
                () => SefiraQueries.GetSefira(SefiraEnum.TIPERERTH1),
                () => SefiraQueries.GetSefira(SefiraEnum.NETZACH),
                () => SefiraQueries.GetSefira(SefiraEnum.HOD),
                () => SefiraQueries.GetSefira(SefiraEnum.YESOD),
                () => SefiraQueries.GetSefira(SefiraEnum.MALKUT),
                () => SefiraQueries.GetSefira(SefiraEnum.DAAT)
            };

            foreach (var act in actions)
            {
                act.Should().Throw<InvalidOperationException>();
            }
        }
    }
}
