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

            var mockCustomizingWindowTestAdapter = new Mock<ICustomizingWindowTestAdapter>();
            var mockGameObjectTestAdapter = new Mock<IGameObjectTestAdapter>();
            var mockUiComponentsTestAdapter = new Mock<IAgentInfoWindowUiComponentsTestAdapter>();

            // Act
            sut.PatchAfterEnforcementWindow(mockCustomizingWindowTestAdapter.Object, mockGameObjectTestAdapter.Object, mockUiComponentsTestAdapter.Object);

            // Assert
            mockCustomizingWindowTestAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockGameObjectTestAdapter.Verify(adapter => adapter.SetActive(true), Times.Exactly(2));
            mockUiComponentsTestAdapter.Verify(adapter => adapter.SetData(It.IsAny<AgentData>()), Times.Once);
        }
    }
}
