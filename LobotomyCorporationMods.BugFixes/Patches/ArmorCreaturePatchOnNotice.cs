// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.BugFixes.Patches
{
    [HarmonyPatch(typeof(ArmorCreature), nameof(ArmorCreature.OnNotice))]
    public static class ArmorCreaturePatchOnNotice
    {
        public static void PatchAfterOnNotice([NotNull] this ArmorCreature instance,
            [NotNull] string noticeName,
            [CanBeNull] IArmorCreatureTestAdapter armorCreatureTestAdapter = null,
            [NotNull] params object[] noticeParameters)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(noticeName, nameof(noticeName));
            ThrowHelper.ThrowIfNull(noticeParameters, nameof(noticeParameters));

            if (!noticeName.Equals(NoticeName.OnChangeGift, StringComparison.Ordinal))
            {
                return;
            }

            instance.ReloadCrumblingArmorAgentList(armorCreatureTestAdapter);
        }

        public static void PostfixWithLogging(Func<ArmorCreature> getArmorCreature, [NotNull] string notice, [NotNull] params object[] param)
        {
            try
            {
                ThrowHelper.ThrowIfNull(getArmorCreature, nameof(getArmorCreature));

                getArmorCreature().PatchAfterOnNotice(notice, noticeParameters: param);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after the original OnNotice method to force Crumbling Armor to re-initialize it's internal list of agents.</summary>
        /// <remarks>
        ///     Bugs fixed:
        ///     <list type="number">
        ///         <item>
        ///             <para>
        ///                 Bug: When an agent that started the day with Crumbling Armor's gift but later replaced the gift with another one, they would still die when performing an
        ///                 Attachment work.
        ///             </para>
        ///             <para>
        ///                 Reproduction: Start the day with an agent that has Crumbling Armor's gift. Have that agent work on One Sin and Hundreds of Good Deeds until they get One Sin's
        ///                 gift, replacing Crumbling Armor's gift. Then have the agent perform an Attachment work on any abnormality other than Crumbling Armor.
        ///             </para>
        ///             <para>Expected result: Agent should not die from Crumbling Armor's effect when starting the Attachment work.</para>
        ///             <para>Actual result: Agent dies from Crumbling Armor's effect.</para>
        ///         </item>
        ///         <item>
        ///             <para>Bug: When an agent with Crumbling Armor's gift replaced a gift in a different slot (e.g. Hand or Face)</para>
        ///             <para>
        ///                 Reproduction: Have an agent with Crumbling Armor's gift replace a gift in a slot that's not a Hat slot gift, the perform Attachment work on any abnormality other
        ///                 than Crumbling Armor.
        ///             </para>
        ///             <para>Expected result: Agent should die from Crumbling Armor's effect when starting the Attachment work.</para>
        ///             <para>Actual result: Agent does not die from Crumbling Armor's effect.</para>
        ///         </item>
        ///     </list>
        ///     <para>
        ///         Technical notes: The root cause of the bug is that the trigger for killing the agent is separate from whether the agent actually has the gift or not. The armor keeps its
        ///         own private list of agents that have either started the day with the gift or have acquired the gift during the day. Unfortunately it doesn't always correctly remove the
        ///         agents from the list when they replace the gift, so the only guaranteed way to avoid the bug normally is to wait until the next day to perform Attachment work. Even
        ///         weirder, there's actually an adjacent bug where replacing ANY gift on that agent triggers removing them from the list, so for example replacing an existing Hand gift will
        ///         also cause the agent to not die from Crumbling Armor's gift (which is on the Head) even though they still have it. This fix will force Crumbling Armor to re-initialize its
        ///         internal list of agents whenever anyone changes their gift so that it will always have the most up-to-date list of agents with the gift and so that we don't have to
        ///         babysit the death trigger.
        ///     </para>
        /// </remarks>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static void Postfix([NotNull] ArmorCreature __instance,
            [NotNull] string notice,
            [NotNull] params object[] param)
        {
            PostfixWithLogging(() => __instance, notice, param);
        }
    }
}
