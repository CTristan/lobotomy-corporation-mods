// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests.AdapterTests
{
    public sealed class GameObjectAdapterTests
    {
        [Fact]
        public void Checking_if_game_object_is_active_returns_status()
        {
            var adapter = new GameObjectAdapter();
            var gameObject = TestExtensions.CreateGameObject();

            var result = adapter.GameObjectIsActive(gameObject);

            result.Should().BeFalse();
        }
    }
}
