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
        [Fact]
        public void Opening_the_agent_window_automatically_opens_the_appearance_window()
        {
            var agentInfoWindow = InitializeAgentInfoWindow();
            var mockUiController = new Mock<IUiController>();
            var mockAgentInfoWindowUiComponentsTestAdapter = new Mock<IAgentInfoWindowUiComponentsTestAdapter>();
            var mockCustomizingWindowTestAdapter = new Mock<ICustomizingWindowTestAdapter>();
            var mockGameObjectTestAdapter = new Mock<IGameObjectTestAdapter>();

            agentInfoWindow.PatchAfterGenerateWindow(mockUiController.Object, mockAgentInfoWindowUiComponentsTestAdapter.Object, mockCustomizingWindowTestAdapter.Object,
                mockGameObjectTestAdapter.Object);

            mockCustomizingWindowTestAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
        }
    }
}
