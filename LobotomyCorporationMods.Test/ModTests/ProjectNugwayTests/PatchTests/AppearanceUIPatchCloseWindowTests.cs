// SPDX-License-Identifier: MIT

#region

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.ProjectNugway.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AppearanceUiPatchCloseWindowTests : ProjectNugwayModTests
    {
        private readonly Mock<AppearanceUI> _sut = new Mock<AppearanceUI>();

        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var result = _sut.Object.PatchBeforeCloseWindow();

            result.Should().BeFalse();
        }
    }
}
