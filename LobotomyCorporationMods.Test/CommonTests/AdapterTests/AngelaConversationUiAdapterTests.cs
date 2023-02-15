// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests.AdapterTests
{
    public sealed class AngelaConversationUiAdapterTests
    {
        [Fact]
        public void Adding_message_does_not_error()
        {
            // Needs existing game instances
            _ = TestExtensions.CreateAngelaConversationUI();
            _ = TestExtensions.CreateGlobalGameManager(GameMode.TUTORIAL);
            _ = TestExtensions.CreateSefiraBossManager(SefiraEnum.DUMMY);
            var adapter = new AngelaConversationUiAdapter();

            Action action = () => adapter.AddMessage(string.Empty);

            action.ShouldNotThrow();
        }
    }
}
