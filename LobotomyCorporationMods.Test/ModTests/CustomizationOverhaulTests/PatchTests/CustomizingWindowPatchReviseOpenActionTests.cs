// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class CustomizingWindowPatchReviseOpenActionTests : CustomizationOverhaulModTests
    {
        [Theory]
        [InlineData("DefaultAgent")]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_gets_agent_appearance_data(string agentName)
        {
            var sut = InitializeCustomizingWindow();
            var agentModel = UnityTestExtensions.CreateAgentModel();
            agentModel.name = agentName;

            sut.PatchAfterReviseOpenAction(agentModel);

            sut.CurrentData.CustomName.Should().Be(agentName);
        }
    }
}
