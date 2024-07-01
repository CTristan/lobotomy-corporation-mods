// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.BugFixes.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), nameof(CustomizingWindow.SetAgentStatBonus))]
    public static class CustomizingWindowPatchSetAgentStatBonus
    {
        public static void PatchBeforeSetAgentStatBonus([NotNull] this CustomizingWindow instance,
            [NotNull] AgentModel agent,
            [NotNull] AgentData data,
            [CanBeNull] ICustomizingWindowAdapter customizingWindowAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(data, nameof(data));

            // This is our custom fixed update
            instance.UpdateAgentStats(agent, data, customizingWindowAdapter);
        }

        /// <summary>Runs before SetAgentStatBonus to use the original stat levels instead of the modified stat levels.</summary>
        /// <remarks>
        ///     <para>
        ///         Bug Fixed: If an agent has gifts that decrease a stat level to a lower level, then leveling up the agent with LOB points will use the lower stat level rather than the
        ///         actual stat level.
        ///     </para>
        ///     <para>
        ///         Reproduction: Test example was an agent with 76 Fortitude at Level 4 but has the Reckless Foolishness gift giving them -20 hp which brings their current Fortitude to
        ///         Level 3. Opening the "Strengthen Employee" window shows Fortitude 4 and allows purchasing an upgrade to Fortitude 5. After purchasing the upgrade, the Fortitude stat
        ///         increases by a few points but still shows Level 3 in the agent window and opening the "Strengthen Employee" window shows the Fortitude at Level 4 again.
        ///     </para>
        ///     <para>
        ///         Expected result: Upgrading the agent's stat Fortitude level should have increased the actual unmodified stat level to the next level instead of remaining at the same
        ///         level.
        ///     </para>
        ///     <para>Actual result: Upgrading the agent's stat Fortitude level used the Level 3 bonus instead of the Level 4 bonus, causing the stat level to remain at Level 4.</para>
        ///     <para>
        ///         Technical notes: Needs to run before the SetAgentStatBonus method runs because the bug is in the stat upgrade calculation itself since we're replacing the calculation
        ///         with the fixed one. If we want to fix it after the original method runs then we would need to make things a lot more complicated to both validate that the upgrade didn't
        ///         work correctly and to perform another upgrade with the correct calculation.
        ///     </para>
        /// </remarks>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable InconsistentNaming
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool Prefix([NotNull] CustomizingWindow __instance,
            [NotNull] AgentModel agent,
            [NotNull] AgentData data)
        {
            try
            {
                __instance.PatchBeforeSetAgentStatBonus(agent, data);

                // Since we're replacing the method we never want to call the original method
                return false;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
