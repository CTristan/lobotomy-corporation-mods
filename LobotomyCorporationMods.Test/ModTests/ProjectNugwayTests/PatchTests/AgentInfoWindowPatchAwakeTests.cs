// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AgentInfoWindowPatchAwakeTests : ProjectNugwayModTests
    {
        [Fact]
        public void UI_components_are_disabled_when_day_is_started()
        {
            // Arrange
            var mockAgentInfoWindow = new Mock<AgentInfoWindow>();
            var mockUiController = new Mock<IUiController>();
            UnityTestExtensions.CreateGameManager();

            // Act
            mockAgentInfoWindow.Object.PatchAfterAwake(mockUiController.Object);

            // Assert
            mockUiController.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Once);
        }

        [Fact]
        public void UI_components_are_not_disabled_when_day_is_not_started()
        {
            // Arrange
            var mockAgentInfoWindow = new Mock<AgentInfoWindow>();
            var mockUiController = new Mock<IUiController>();
            UnityTestExtensions.CreateGameManager(GameState.STOP);

            // Act
            mockAgentInfoWindow.Object.PatchAfterAwake(mockUiController.Object);

            // Assert
            mockUiController.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Never);
        }
    }
}
