// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AgentInfoWindowPatchCloseWindowTests : ProjectNugwayModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();
        private readonly Mock<IUiController> _uiControllerMock = new Mock<IUiController>();

        [Fact]
        public void Closing_agent_info_window_disables_UI_controls()
        {
            _sut.Object.PatchAfterCloseWindow(_uiControllerMock.Object);
            _uiControllerMock.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Once);
        }
    }
}
