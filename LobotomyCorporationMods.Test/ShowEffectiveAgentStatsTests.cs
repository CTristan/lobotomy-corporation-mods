using LobotomyCorporation.ShowEffectiveAgentStats;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    public sealed class ShowEffectiveAgentStatsTests
    {
        private const long AgentId = 1;

        public ShowEffectiveAgentStatsTests()
        {
            const string DataPath = @"./";
            _ = new Harmony_Patch(DataPath);

            AgentInfoWindow.currentWindow = new AgentInfoWindow
            {
                Additional_Plus_ValueColor = "blue", Additional_Minus_ValueColor = "red"
            };
        }

        [Theory]
        [InlineData(1, 0, 0f, "1")]
        [InlineData(10, 1, 0f, "11/11 (10<color=#blue>+1</color>+0)")]
        [InlineData(10, -1, 0f, "9/9 (10<color=#red>-1</color>+0)")]
        [InlineData(100, 10, 1f, "110/111 (100<color=#blue>+10</color>+1)")]
        public void SetStat_IsRedStat_ReturnsCorrectString(int primaryStat, int egoBonus, float workExp,
            string expectedString)
        {
            // Arrange
            const RwbpType Type = RwbpType.R;
            var workerPrimaryStatUI = TestExtensions.CreateAgentInfoWindow_WorkerPrimaryStatUI(Type);

            var agentModel = TestExtensions.CreateAgentModel(AgentId, primaryStat, egoBonus, workExp);

            // Act
            Harmony_Patch.SetStat(workerPrimaryStatUI, agentModel);

            // Assert
            Assert.Equal(expectedString, workerPrimaryStatUI.list[0].StatValue.text);
        }
    }
}
