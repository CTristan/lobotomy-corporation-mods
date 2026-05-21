// SPDX-License-Identifier: MIT

#region
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class AgentInfoWindowPatchGenerateWindowTests : CustomizationOverhaulModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();

        [Fact]
        public void Opening_the_agent_window_automatically_opens_the_appearance_window()
        {
            InitializeCustomizingWindow();

            var mockAgentInfoWindowUiComponentsInternals =
                new Mock<IAgentInfoWindowUiComponentsInternals>();
            var mockCustomizingWindowInternals = new Mock<ICustomizingWindowInternals>();
            var mockCustomizingBlockInternals = new Mock<IGameObjectInternals>();
            var mockAppearanceControlInternals = new Mock<IGameObjectInternals>();

            _sut.Object.PatchAfterGenerateWindow(
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
