// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class CustomizeAgentUiFacade
    {
        public static void UpdateAgentStats(this CustomizingWindow customizingWindow,
            [NotNull] AgentModel agent,
            [NotNull] AgentData agentData,
            ICustomizingWindowTestAdapter customizingWindowTestAdapter = null)
        {
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(agentData, nameof(agentData));
            customizingWindowTestAdapter = customizingWindowTestAdapter.EnsureNotNullWithMethod(() => new CustomizingWindowTestAdapter());

            agent.primaryStat.hp = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.hp, agent.originFortitudeLevel, agentData.statBonus.rBonus);
            agent.primaryStat.mental = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.mental, agent.originPrudenceLevel, agentData.statBonus.wBonus);
            agent.primaryStat.work = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.work, agent.originTemperanceLevel, agentData.statBonus.bBonus);
            agent.primaryStat.battle = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.battle, agent.originJusticeLevel, agentData.statBonus.pBonus);
            agent.UpdateTitle(agent.level);
        }
    }
}
