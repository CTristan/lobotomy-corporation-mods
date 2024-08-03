// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    /// <summary>Contains tests for verifying the behavior of the AgentInfoWindow patch in the ProjectNugway mod.</summary>
    public sealed class AgentInfoWindowPatchEnforcementWindowTests : ProjectNugwayModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();

        /// <summary>Tests that opening the strengthen employee window also opens the Appearance UI.</summary>
        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            // Arrange
            InitializeCustomizingWindow();

            var mockUiController = new Mock<IUiController>();
            var mockAgentInfoWindowUiComponents = new Mock<IAgentInfoWindowUiComponentsTestAdapter>();
            var mockCustomizingWindow = new Mock<ICustomizingWindowTestAdapter>();
            var mockGameObject = new Mock<IGameObjectTestAdapter>();

            // Act
            _sut.Object.PatchAfterEnforcementWindow(mockUiController.Object, mockAgentInfoWindowUiComponents.Object, mockCustomizingWindow.Object, mockGameObject.Object);

            // Assert
            mockAgentInfoWindowUiComponents.Verify(adapter => adapter.SetData(It.IsAny<AgentData>()), Times.Once);
            mockCustomizingWindow.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockGameObject.Verify(adapter => adapter.SetActive(true), Times.Exactly(2));
        }
    }
}
