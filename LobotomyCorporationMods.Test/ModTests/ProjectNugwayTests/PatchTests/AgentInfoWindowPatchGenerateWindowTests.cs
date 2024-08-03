// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AgentInfoWindowPatchGenerateWindowTests : ProjectNugwayModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();

        [Fact]
        public void Opening_the_agent_window_automatically_opens_the_appearance_window()
        {
            InitializeCustomizingWindow();

            var mockUiController = new Mock<IUiController>();
            var mockAgentInfoWindowUiComponentsTestAdapter = new Mock<IAgentInfoWindowUiComponentsTestAdapter>();
            var mockCustomizingWindowTestAdapter = new Mock<ICustomizingWindowTestAdapter>();
            var mockGameObjectTestAdapter = new Mock<IGameObjectTestAdapter>();

            _sut.Object.PatchAfterGenerateWindow(mockUiController.Object, mockAgentInfoWindowUiComponentsTestAdapter.Object, mockCustomizingWindowTestAdapter.Object, mockGameObjectTestAdapter.Object);

            mockCustomizingWindowTestAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
        }
    }
}
