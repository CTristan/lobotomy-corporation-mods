// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common.Interfaces.Adapters;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters.BaseClasses;
using Hemocode.FreeCustomization.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.FreeCustomizationTests.PatchTests
{
    public sealed class AgentInfoWindowPatchGenerateWindowTests : FreeCustomizationModTests
    {
        [Fact]
        public void Opening_the_agent_window_automatically_opens_the_appearance_window()
        {
            var agentInfoWindow = InitializeAgentInfoWindow();
            Mock<IAgentInfoWindowUiComponentsTestAdapter> mockAgentInfoWindowUiComponentsTestAdapter = new();
            Mock<ICustomizingWindowTestAdapter> mockCustomizingWindowTestAdapter = new();
            Mock<IGameObjectTestAdapter> mockGameObjectTestAdapter = new();

            agentInfoWindow.PatchAfterGenerateWindow(mockAgentInfoWindowUiComponentsTestAdapter.Object, mockCustomizingWindowTestAdapter.Object, mockGameObjectTestAdapter.Object);

            mockCustomizingWindowTestAdapter.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
        }
    }
}
