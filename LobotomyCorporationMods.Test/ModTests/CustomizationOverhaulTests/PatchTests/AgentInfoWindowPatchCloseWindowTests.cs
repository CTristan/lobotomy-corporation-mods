// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class AgentInfoWindowPatchCloseWindowTests : CustomizationOverhaulModTests
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
