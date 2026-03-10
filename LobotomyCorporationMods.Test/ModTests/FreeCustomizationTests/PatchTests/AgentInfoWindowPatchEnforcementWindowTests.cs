// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
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

            Mock<IAgentInfoWindowUiComponentsTestAdapter> mockAgentInfoWindowUiComponentsTestAdapter = new();
            Mock<ICustomizingWindowTestAdapter> mockCustomizingWindowTestAdapter = new();
            Mock<IGameObjectTestAdapter> mockGameObjectTestAdapter = new();

            // Act
            sut.PatchAfterEnforcementWindow(mockAgentInfoWindowUiComponentsTestAdapter.Object, mockCustomizingWindowTestAdapter.Object, mockGameObjectTestAdapter.Object);

            // Assert
            mockAgentInfoWindowUiComponentsTestAdapter.Verify(adapter => adapter.SetData(It.IsAny<AgentData>()), Times.Once);
            mockCustomizingWindowTestAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockGameObjectTestAdapter.Verify(adapter => adapter.SetActive(true), Times.Exactly(Twice));
        }
    }
}
