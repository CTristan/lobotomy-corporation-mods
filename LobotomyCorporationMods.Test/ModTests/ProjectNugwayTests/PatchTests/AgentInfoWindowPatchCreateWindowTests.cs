// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AgentInfoWindowPatchCreateWindowTests : ProjectNugwayModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();
        private readonly Mock<IUiController> _uiControllerMock = new Mock<IUiController>();

        [Theory]
        [InlineData(GameState.PLAYING)]
        [InlineData(GameState.PAUSE)]
        public void UI_components_are_disabled_when_day_is_started(GameState gameState)
        {
            // Arrange
            UnityTestExtensions.CreateGameManager(gameState);

            // Act
            _sut.Object.PatchAfterCreateWindow(_uiControllerMock.Object);

            // Assert
            _uiControllerMock.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Once);
        }

        [Fact]
        public void UI_components_are_not_disabled_when_day_is_stopped()
        {
            // Arrange
            UnityTestExtensions.CreateGameManager(GameState.STOP);

            // Act
            _sut.Object.PatchAfterCreateWindow(_uiControllerMock.Object);

            // Assert
            _uiControllerMock.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Never);
        }
    }
}
