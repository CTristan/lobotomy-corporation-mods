// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class AgentInfoWindowPatchEnforcementWindowTests : FreeCustomizationTests
    {
        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            // Arrange
            var sut = InitializeAgentInfoWindow();
            _ = InitializeCustomizingWindow();

            var mockCustomizingWindowAdapter = new Mock<ICustomizingWindowAdapter>();
            var mockGameObjectAdapter = new Mock<IGameObjectAdapter>();
            var mockUiComponentsAdapter = new Mock<IAgentInfoWindowUiComponentsAdapter>();

            // Act
            sut.PatchAfterEnforcementWindow(mockCustomizingWindowAdapter.Object, mockGameObjectAdapter.Object, mockUiComponentsAdapter.Object);

            // Assert
            mockCustomizingWindowAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockGameObjectAdapter.Verify(adapter => adapter.SetActive(true), Times.Exactly(2));
            mockUiComponentsAdapter.Verify(adapter => adapter.SetData(It.IsAny<AgentData>()), Times.Once);
        }
    }
}
