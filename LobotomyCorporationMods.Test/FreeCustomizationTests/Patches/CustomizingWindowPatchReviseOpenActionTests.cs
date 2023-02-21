// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.FreeCustomizationTests.Patches
{
    public sealed class CustomizingWindowPatchReviseOpenActionTests : FreeCustomizationTests
    {
        [Theory]
        [InlineData("DefaultAgent")]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_gets_agent_appearance_data(string agentName)
        {
            var customizingWindow = GetCustomizingWindow();
            var agentModel = TestExtensions.CreateAgentModel();
            agentModel.name = agentName;

            CustomizingWindowPatchReviseOpenAction.Postfix(customizingWindow, agentModel);

            customizingWindow.CurrentData.CustomName.Should().Be(agentName);
        }
    }
}
