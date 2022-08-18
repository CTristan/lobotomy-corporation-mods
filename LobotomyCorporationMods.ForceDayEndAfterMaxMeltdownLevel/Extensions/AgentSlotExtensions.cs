// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions;

// ReSharper disable once CheckNamespace
namespace CommandWindow
{
    public static class AgentSlotExtensions
    {
        public static bool IsMaxMeltdown([NotNull] this AgentSlot agentSlot, AgentState agentState, [NotNull] CommandWindow commandWindow, CreatureOverloadManager creatureOverloadManager,
            PlayerModel playerModel, StageTypeInfo stageTypeInfo, EnergyModel energyModel)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            if (agentState.IsUncontrollable() || !commandWindow.IsValid() || !playerModel.HasEnoughEnergy(stageTypeInfo, energyModel))
            {
                return false;
            }

            if (!commandWindow.TryGetCreatureModel(out var creatureModel) || creatureModel == null)
            {
                return false;
            }

            return creatureOverloadManager.IsMaxMeltdown() && !creatureModel.MeltdownActivated();
        }
    }
}
