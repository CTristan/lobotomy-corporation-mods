// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class CustomizingWindowPatchReviseOpenActionTests : FreeCustomizationTests
    {
        [Theory]
        [InlineData("DefaultAgent")]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_gets_agent_appearance_data(string agentName)
        {
            var sut = InitializeCustomizingWindow();
            var agentModel = TestUnityExtensions.CreateAgentModel();
            agentModel.name = agentName;

            sut.PatchAfterReviseOpenAction(agentModel);

            sut.CurrentData.CustomName.Should().Be(agentName);
        }
    }
}
