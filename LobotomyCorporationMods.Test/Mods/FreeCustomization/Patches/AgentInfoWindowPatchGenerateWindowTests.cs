// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class AgentInfoWindowPatchGenerateWindowTests : FreeCustomizationTests
    {
        [Fact]
        public void Opening_the_agent_window_automatically_opens_the_appearance_window()
        {
            var agentInfoWindow = GetAgentInfoWindow();
            var mockCustomizingWindowAdapter = new Mock<ICustomizingWindowAdapter>();

            agentInfoWindow.PatchAfterGenerateWindow(mockCustomizingWindowAdapter.Object);

            mockCustomizingWindowAdapter.Verify(static adapter => adapter.OpenAppearanceWindow(), Times.Once);
        }
    }
}
