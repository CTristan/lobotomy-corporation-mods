// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AppearanceUiPatchCloseWindowTests : ProjectNugwayModTests
    {
        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = UnityTestExtensions.CreateAppearanceUi();

            var result = appearanceUi.PatchBeforeCloseWindow();

            result.Should().BeFalse();
        }
    }
}
