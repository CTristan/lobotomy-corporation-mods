// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions;

// ReSharper disable once CheckNamespace
namespace CommandWindow
{
    public static class AgentSlotExtensions
    {
        internal static bool IsMaxMeltdown([NotNull] this AgentSlot agentSlot, [NotNull] CommandWindow commandWindow, CreatureOverloadManager creatureOverloadManager)
        {
            return agentSlot.IsMaxMeltdown(AgentState.IDLE, commandWindow, creatureOverloadManager);
        }

        public static bool IsMaxMeltdown([NotNull] this AgentSlot agentSlot, AgentState state, [NotNull] CommandWindow commandWindow, CreatureOverloadManager creatureOverloadManager)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            if (!commandWindow.TryGetCreatureModel(out var creatureModel) || creatureModel == null)
            {
                return false;
            }

            if (state.IsUncontrollable() || !commandWindow.IsValid() || !creatureOverloadManager.IsMaxMeltdown() || creatureModel.IsRoomInMeltdown())
            {
                return false;
            }

            return true;
        }
    }
}
