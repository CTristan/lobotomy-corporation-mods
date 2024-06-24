// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.BugFixes.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "SetAgentStatBonus")]
    public static class CustomizingWindowPatchSetAgentStatBonus
    {
        public static void PatchBeforeSetAgentStatBonus(this CustomizingWindow instance, AgentModel agent,
            AgentData data, ICustomizingWindowAdapter customizingWindowAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(data, nameof(data));
            Guard.Against.Null(customizingWindowAdapter, nameof(customizingWindowAdapter));

            customizingWindowAdapter.GameObject = instance;
            agent.primaryStat.hp = customizingWindowAdapter.SetRandomStatValue(agent.primaryStat.hp,
                agent.originFortitudeLevel, data.statBonus.rBonus);
            agent.primaryStat.mental = customizingWindowAdapter.SetRandomStatValue(agent.primaryStat.mental,
                agent.originPrudenceLevel, data.statBonus.wBonus);
            agent.primaryStat.work = customizingWindowAdapter.SetRandomStatValue(agent.primaryStat.work,
                agent.originTemperanceLevel, data.statBonus.bBonus);
            agent.primaryStat.battle = customizingWindowAdapter.SetRandomStatValue(agent.primaryStat.battle,
                agent.originJusticeLevel, data.statBonus.pBonus);
            agent.UpdateTitle(agent.level);
        }

        /// <summary>
        ///     Runs before SetAgentStatBonus to use the original stat levels instead of the modified stat levels.
        ///
        ///     Bug Fixed: If an agent has gifts that decrease a stat level to a lower level, then leveling up the agent
        ///     with LOB points will use the lower stat level rather than the actual stat level.
        ///
        ///     Reproduction: Test example was an agent with 76 Fortitude at Level 4 but has the Reckless Foolishness
        ///     gift giving them -20 hp which brings their current Fortitude to Level 3. Opening the "Strengthen
        ///     Employee" window shows Fortitude 4 and allows purchasing an upgrade to Fortitude 5. After purchasing the
        ///     upgrade, the Fortitude stat increases by a few points but still shows Level 3 in the agent window and
        ///     opening the "Strengthen Employee" window shows the Fortitude at Level 4 again.
        ///
        ///     Expected result: Upgrading the agent's stat Fortitude level should have increased the actual unmodified
        ///     stat level to the next level instead of remaining at the same level.
        ///
        ///     Actual result: Upgrading the agent's stat Fortitude level used the Level 3 bonus instead of the Level 4
        ///     bonus, causing the stat level to remain at Level 4.
        ///
        ///     Needs to run before the SetAgentStatBonus method runs because the bug is in the stat upgrade calculation
        ///     itself and we're replacing the calculation with the fixed one. If we want to fix it after the original
        ///     method runs then we would need to make things a lot more complicated to both validate that the upgrade
        ///     didn't work correctly and to perform another upgrade with the correct calculation.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static bool Prefix(CustomizingWindow __instance, AgentModel agent, AgentData data)
        {
            try
            {
                __instance.PatchBeforeSetAgentStatBonus(agent, data, new CustomizingWindowAdapter());

                // Since we're replacing the method we never want to call the original method
                return false;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
