// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), GameMethods.UseSkill.FinishWorkSuccessfully)]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
        /// <summary>Captures the current working agent's ID before FinishWorkSuccessfully runs,
        /// so that GetProb can look up the correct agent's bonus during the gift probability check.</summary>
        public static long PatchBeforeFinishWorkSuccessfully([NotNull] this UseSkill instance)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            return instance.GetAgentId();
        }

        /// <summary>Checks whether the agent already has the abnormality's gift before work begins.
        /// Used so that ResetOnGiftReceived only fires on the work session where the agent first
        /// receives the gift, not on every subsequent session where they already have it.</summary>
        public static bool DidAgentAlreadyHaveAbnormalityGift([NotNull] this UseSkill instance)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            var giftId = instance.GetAbnormalityGiftId();

            return giftId.HasValue && instance.agent.HasGift(giftId);
        }

        public static void PatchAfterFinishWorkSuccessfully(
            [NotNull] this UseSkill instance,
            [NotNull] IAgentWorkTracker agentWorkTracker,
            [NotNull] IBadLuckProtectionConfig config,
            bool hadGiftBeforeWork
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));
            ThrowHelper.ThrowIfNull(config, nameof(config));

            var giftName = instance.GetAbnormalityGiftName();

            // If the abnormality has no gift then there's nothing to track
            if (string.IsNullOrEmpty(giftName))
            {
                return;
            }

            var riskLevel = instance.GetAbnormalityRiskLevel();
            agentWorkTracker.SetRiskLevelForGift(giftName, riskLevel);

            var agentId = instance.GetAgentId();

            float incrementValue;
            if (config.BonusCalculationMode == BonusCalculationMode.Normalized)
            {
                var maxCubeCount = instance.maxCubeCount;
                incrementValue =
                    maxCubeCount > 0 ? (float)instance.successCount / maxCubeCount : 0f;
            }
            else
            {
                incrementValue = instance.successCount;
            }

            agentWorkTracker.IncrementAgentWorkCount(giftName, agentId, incrementValue);

            if (config.ResetOnGiftReceived && !hadGiftBeforeWork)
            {
                var giftId = instance.GetAbnormalityGiftId();
                if (giftId.HasValue && instance.agent.HasGift(giftId))
                {
                    agentWorkTracker.ResetAgentWorkCountForGift(giftName, agentId);
                }
            }
        }

        /// <summary>Runs before FinishWorkSuccessfully to capture the current agent's ID and
        /// whether the agent already has the abnormality's gift. Prefix is required because
        /// GetProb() is called inside the original method body, and the GetProb patch needs
        /// to know which agent is currently working. The gift state must be captured before
        /// the work runs because the work itself can grant the gift.
        ///
        /// Any stale captured state is cleared at the start of this method. The game ships
        /// Harmony 1.0.9.1, which does not support HarmonyFinalizer, so if the original
        /// FinishWorkSuccessfully throws, the Postfix (and its finally block) will not run
        /// and the captured agent ID could otherwise leak into later GetProb calls.
        /// Clearing at the start of the next Prefix bounds that leak to the period between
        /// the failed call and the next FinishWorkSuccessfully call.</summary>
        /// <param name="__instance"></param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix([NotNull] UseSkill __instance)
        {
            Harmony_Patch.Instance.CurrentWorkingAgentId = null;
            Harmony_Patch.Instance.CurrentWorkingAgentHadGiftBeforeWork = false;

            try
            {
                Harmony_Patch.Instance.CurrentWorkingAgentId =
                    __instance.PatchBeforeFinishWorkSuccessfully();
                Harmony_Patch.Instance.CurrentWorkingAgentHadGiftBeforeWork =
                    __instance.DidAgentAlreadyHaveAbnormalityGift();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after an agent finishes working with an abnormality to increment their work count.</summary>
        /// <param name="__instance"></param>
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] UseSkill __instance)
        {
            try
            {
                __instance.PatchAfterFinishWorkSuccessfully(
                    Harmony_Patch.Instance.AgentWorkTracker,
                    Harmony_Patch.Instance.Config,
                    Harmony_Patch.Instance.CurrentWorkingAgentHadGiftBeforeWork
                );
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
            finally
            {
                Harmony_Patch.Instance.CurrentWorkingAgentId = null;
                Harmony_Patch.Instance.CurrentWorkingAgentHadGiftBeforeWork = false;
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
