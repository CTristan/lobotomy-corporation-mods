// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions;

// ReSharper disable once CheckNamespace
namespace CommandWindow
{
    public static class AgentSlotExtensions
    {
        public static void DisableIfMaxMeltdown([NotNull] this AgentSlot agentSlot, AgentState state, [NotNull] CommandWindow commandWindow, CreatureOverloadManager creatureOverloadManager)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            if (!commandWindow.TryGetCreatureModel(out var creatureModel) || creatureModel == null)
            {
                return;
            }

            if (state.IsUncontrollable() || !commandWindow.IsValid() || !creatureOverloadManager.IsMaxMeltdown() || creatureModel.IsRoomInMeltdown())
            {
                return;
            }

            // Now that we're past all of our checks, disallow working on this slot
            agentSlot.SetColor(commandWindow.UnconColor);
            agentSlot.State = AgentState.UNCONTROLLABLE;
        }
    }
}
