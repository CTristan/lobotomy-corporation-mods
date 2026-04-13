// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.FreeCustomizationTests.PatchTests
{
    public sealed class AgentInfoWindowPatchEnforcementWindowTests : FreeCustomizationModTests
    {
        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            // Arrange
            var sut = InitializeAgentInfoWindow();
            _ = InitializeCustomizingWindow();

            var mockAgentInfoWindowUiComponentsInternals =
                new Mock<IAgentInfoWindowUiComponentsInternals>();
            var mockCustomizingWindowInternals = new Mock<ICustomizingWindowInternals>();
            var mockCustomizingBlockInternals = new Mock<IGameObjectInternals>();
            var mockAppearanceControlInternals = new Mock<IGameObjectInternals>();

            // Act
            sut.PatchAfterEnforcementWindow(
                mockAgentInfoWindowUiComponentsInternals.Object,
                mockCustomizingWindowInternals.Object,
                mockCustomizingBlockInternals.Object,
                mockAppearanceControlInternals.Object
            );

            // Assert
            mockAgentInfoWindowUiComponentsInternals.Verify(
                adapter => adapter.SetData(It.IsAny<AgentData>()),
                Times.Once
            );
            mockCustomizingWindowInternals.Verify(
                adapter => adapter.OpenAppearanceWindow(),
                Times.Once
            );
            mockCustomizingBlockInternals.Verify(adapter => adapter.SetActive(true), Times.Once);
            mockAppearanceControlInternals.Verify(adapter => adapter.SetActive(true), Times.Once);
        }
    }
}
