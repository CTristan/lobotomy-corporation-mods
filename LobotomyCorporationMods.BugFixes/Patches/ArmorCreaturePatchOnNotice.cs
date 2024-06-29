// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BugFixes.Patches
{
    [HarmonyPatch(typeof(ArmorCreature), nameof(ArmorCreature.OnNotice))]
    public static class ArmorCreaturePatchOnNotice
    {
        public static bool PatchBeforeOnNotice(string notice,
            [NotNull] params object[] param)
        {
            Guard.Against.Null(param, nameof(param));

            if (notice != NoticeName.OnWorkStart)
            {
                return true;
            }

            // If we're working on a tool or other non-creature then we don't need to verify
            if (!(param[0] is CreatureModel creatureModel))
            {
                return true;
            }

            // We only care if we're doing Attachment work
            var skillId = creatureModel.currentSkill.skillTypeInfo.id;
            if (skillId != SkillTypeInfo.Consensus)
            {
                return true;
            }

            var agent = creatureModel.currentSkill.agent;

            // If the agent doesn't actually have Crumbling Armor's gift then we won't continue.
            return agent.HasCrumblingArmor();
        }

        /// <summary>Runs before the original OnNotice method to prevent the method from running if we detect that the agent no longer has Crumbling Armor's gift.</summary>
        /// <remarks>
        ///     <para>
        ///         Bug Fixed: When an agent that started the day with Crumbling Armor's gift but later replaced the gift with another one, they would still die when performing an
        ///         Attachment work.
        ///     </para>
        ///     <para>
        ///         Reproduction: Start the day with an agent that has Crumbling Armor's gift. Have that agent work on One Sin and Hundreds of Good Deeds until they get One Sin's gift,
        ///         replacing Crumbling Armor's gift. Then have the agent perform an Attachment work on any abnormality.
        ///     </para>
        ///     <para>Expected result: Agent should not die from Crumbling Armor's effect when starting the Attachment work.</para>
        ///     <para>Actual result: Agent dies from Crumbling Armor's effect.</para>
        ///     <para>
        ///         Technical notes: The root cause of the bug is that the trigger for killing the agent is separate from whether the agent actually has the gift or not. The armor keeps its
        ///         own private list of agents that have either started the day with the gift or have acquired the gift during the day. Unfortunately it doesn't correctly remove the agents
        ///         from the list when they replace the gift, so the only way to avoid the bug normally is to wait until the next day to perform Attachment work. This fix will force the
        ///         trigger to check if the agent actually has the gift, and if they do then we stop the armor from checking its private list for the agent. This needs to be Prefix because
        ///         the bug is in a private (and thus inaccessible) property, so we need to be able to skip the method call to work around that issue.
        ///     </para>
        /// </remarks>
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool Prefix(string notice,
            [NotNull] params object[] param)
        {
            try
            {
                return PatchBeforeOnNotice(notice, param);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
