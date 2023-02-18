// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.FreeCustomizationTests.Patches
{
    public sealed class AgentInfoWindowPatchEnforcementWindowTests : FreeCustomizationTests
    {
        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            // Arrange
            _ = GetCustomizingWindow();
            InitializeAgentInfoWindow();

            var mockAgentInfoWindowUiComponentsAdapter = new Mock<IAgentInfoWindowUiComponentsAdapter>();
            AgentInfoWindowPatchEnforcementWindow.InfoWindowUiComponentsAdapter = mockAgentInfoWindowUiComponentsAdapter.Object;

            var mockAgentInfoWindowCustomizingWindowAdapter = new Mock<ICustomizingWindowAdapter>();
            AgentInfoWindowPatchEnforcementWindow.AgentInfoWindowCustomizingWindowAdapter = mockAgentInfoWindowCustomizingWindowAdapter.Object;

            var mockGameObjectAdapter = new Mock<IGameObjectAdapter>();
            AgentInfoWindowPatchEnforcementWindow.GameObjectAppearanceActiveControlAdapter = mockGameObjectAdapter.Object;
            AgentInfoWindowPatchEnforcementWindow.GameObjectCustomizingBlockAdapter = mockGameObjectAdapter.Object;

            // Act
            AgentInfoWindowPatchEnforcementWindow.Postfix();

            // Assert
            mockGameObjectAdapter.Verify(static adapter => adapter.SetActive(true), Times.Exactly(2));
            mockAgentInfoWindowCustomizingWindowAdapter.Verify(static adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockAgentInfoWindowUiComponentsAdapter.Verify(static adapter => adapter.SetData(It.IsAny<AgentData>()), Times.Once);
        }
    }
}
