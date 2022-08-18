// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions
{
    public static class AgentModelExtensions
    {
        public static bool CheckIfMaxMeltdown([NotNull] this AgentModel agentModel, [NotNull] CreatureOverloadManager creatureOverloadManager, CreatureModel creatureModel, PlayerModel playerModel,
            StageTypeInfo stageTypeInfo, EnergyModel energyModel)
        {
            Guard.Against.Null(agentModel, nameof(agentModel));
            Guard.Against.Null(creatureOverloadManager, nameof(creatureOverloadManager));
            Guard.Against.Null(creatureModel, nameof(creatureModel));

            return creatureOverloadManager.IsMaxMeltdown() && !creatureModel.MeltdownActivated() && playerModel.HasEnoughEnergy(stageTypeInfo, energyModel);
        }
    }
}
