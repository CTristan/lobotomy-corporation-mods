// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

namespace LobotomyCorporationMods.Test.CommonTests.AdapterTests
{
    public sealed class GameObjectAdapterTests
    {
        [Fact]
        public void Checking_if_game_object_is_active_throws_Unity_exception()
        {
            var adapter = new GameObjectAdapter();
            var gameObject = TestExtensions.CreateGameObject();

            Action action = () => _ = adapter.GameObjectIsActive(gameObject);

            action.ShouldThrowUnityException();
        }
    }
}
