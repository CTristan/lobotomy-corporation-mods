// SPDX-License-Identifier: MIT

#region

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.FreeCustomization.Patches;
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
            var mockAgentInfoWindowUiComponentsInternals =
                new Mock<IAgentInfoWindowUiComponentsInternals>();
            var mockCustomizingWindowInternals = new Mock<ICustomizingWindowInternals>();
            var mockCustomizingBlockInternals = new Mock<IGameObjectInternals>();
            var mockAppearanceControlInternals = new Mock<IGameObjectInternals>();

            agentInfoWindow.PatchAfterGenerateWindow(
                mockAgentInfoWindowUiComponentsInternals.Object,
                mockCustomizingWindowInternals.Object,
                mockCustomizingBlockInternals.Object,
                mockAppearanceControlInternals.Object
            );

            mockCustomizingWindowInternals.Verify(
                adapter => adapter.OpenAppearanceWindow(),
                Times.Once
            );
        }
    }
}
