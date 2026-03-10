// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using Customizing;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.FreeCustomizationTests.PatchTests
{
    public sealed class AppearanceUiPatchCloseWindowTests : FreeCustomizationModTests
    {
        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = UnityTestExtensions.CreateAppearanceUi();

            var result = appearanceUi.PatchBeforeCloseWindow();

            _ = result.Should().BeFalse();
        }
    }
}
