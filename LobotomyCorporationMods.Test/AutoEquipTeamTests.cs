using LobotomyCorporationMods.AutoEquipTeam;
using Xunit;

namespace LobotomyCorporationMods.Test
{
    public class AutoEquipTeamTests
    {
        public AutoEquipTeamTests()
        {
            var fileManager = TestExtensions.GetFileManager();
            _ = new Harmony_Patch(fileManager);
        }

        [Fact]
        public void EquipAllAgents_()
        {
            // Arrange

            // Act
            // Harmony_Patch.EquipAllAgents();

            // Assert
        }
    }
}
