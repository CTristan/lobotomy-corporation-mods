// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using Moq;
using Xunit;

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class CustomizingWindowPatchCancelTests : ProjectNugwayModTests
    {
        private readonly Mock<CustomizingWindow> _sut = new Mock<CustomizingWindow>();
        private readonly Mock<IUiController> _uiControllerMock = new Mock<IUiController>();

        [Fact]
        public void Cancelling_customization_disables_UI_components()
        {
            _sut.Object.PatchAfterCancel(_uiControllerMock.Object);
            _uiControllerMock.Verify(ui => ui.DisableAllCustomUiComponents(), Times.Once);
        }
    }
}
