// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class AgentInfoWindowPatchAwakeTests : CustomizationOverhaulModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();

        [Fact]
        public void UI_components_are_disabled_when_day_is_started()
        {
            // Arrange
            var mockUiController = new Mock<IUiController>();
            UnityTestExtensions.CreateGameManager();

            // Act
            _sut.Object.PatchAfterAwake(mockUiController.Object);

            // Assert
            mockUiController.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Once);
        }

        [Fact]
        public void UI_components_are_not_disabled_when_day_is_not_started()
        {
            // Arrange
            var mockUiController = new Mock<IUiController>();
            UnityTestExtensions.CreateGameManager();
            GameManager.currentGameManager.state = GameState.STOP;

            // Act
            _sut.Object.PatchAfterAwake(mockUiController.Object);

            // Assert
            mockUiController.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Never);
        }
    }
}
