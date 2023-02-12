// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.BugFixes.Extensions
{
    public static class CustomizingWindowExtensions
    {
        internal static void UpgradeAgentStats([NotNull] this CustomizingWindow customizingWindow, [NotNull] AgentModel agent, [NotNull] AgentData data)
        {
            UpgradeAgentStats(customizingWindow, agent, data, new CustomizingWindowAdapter(customizingWindow));
        }

        public static void UpgradeAgentStats([NotNull] this CustomizingWindow customizingWindow, [NotNull] AgentModel agent, [NotNull] AgentData data, ICustomizingWindowAdapter adapter)
        {
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(data, nameof(data));

            adapter ??= new CustomizingWindowAdapter(customizingWindow);

            agent.primaryStat.hp = adapter.UpgradeAgentStat(agent.primaryStat.hp, agent.originFortitudeLevel, data.statBonus.rBonus);
            agent.primaryStat.mental = adapter.UpgradeAgentStat(agent.primaryStat.mental, agent.originPrudenceLevel, data.statBonus.wBonus);
            agent.primaryStat.work = adapter.UpgradeAgentStat(agent.primaryStat.work, agent.originTemperanceLevel, data.statBonus.bBonus);
            agent.primaryStat.battle = adapter.UpgradeAgentStat(agent.primaryStat.battle, agent.originJusticeLevel, data.statBonus.pBonus);
            agent.UpdateTitle(agent.level);
        }
    }
}
