// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    /// <summary>Contains tests for verifying the behavior of the AgentInfoWindow patch in the CustomizationOverhaul mod.</summary>
    public sealed class AgentInfoWindowPatchEnforcementWindowTests : CustomizationOverhaulModTests
    {
        private readonly Mock<AgentInfoWindow> _sut = new Mock<AgentInfoWindow>();

        /// <summary>Tests that opening the strengthen employee window also opens the Appearance UI.</summary>
        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            // Arrange
            InitializeCustomizingWindow();

            var mockAgentInfoWindowUiComponents = new Mock<IAgentInfoWindowUiComponentsInternals>();
            var mockCustomizingWindow = new Mock<ICustomizingWindowInternals>();
            var mockCustomizingBlock = new Mock<IGameObjectInternals>();
            var mockAppearanceControl = new Mock<IGameObjectInternals>();

            // Act
            _sut.Object.PatchAfterEnforcementWindow(
                mockAgentInfoWindowUiComponents.Object,
                mockCustomizingWindow.Object,
                mockCustomizingBlock.Object,
                mockAppearanceControl.Object
            );

            // Assert
            mockAgentInfoWindowUiComponents.Verify(
                adapter => adapter.SetData(It.IsAny<AgentData>()),
                Times.Once
            );
            mockCustomizingWindow.Verify(adapter => adapter.OpenAppearanceWindow(), Times.Once);
            mockCustomizingBlock.Verify(adapter => adapter.SetActive(true), Times.Once);
            mockAppearanceControl.Verify(adapter => adapter.SetActive(true), Times.Once);
        }
    }
}
