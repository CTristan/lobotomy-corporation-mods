// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class AppearanceUIPatchCloseWindowTests : FreeCustomizationTests
    {
        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = TestExtensions.CreateAppearanceUI();

            var result = appearanceUi.PatchBeforeCloseWindow();

            result.Should().BeFalse();
        }
    }
}
