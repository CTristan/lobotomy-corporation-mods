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

        public static void PatchAfterFinishWorkSuccessfully(
            [NotNull] this UseSkill instance,
            [NotNull] IAgentWorkTracker agentWorkTracker,
            [NotNull] IBadLuckProtectionConfig config
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

            if (config.ResetOnGiftReceived)
            {
                var giftId = instance.GetAbnormalityGiftId();
                if (giftId.HasValue && instance.agent.HasGift(giftId))
                {
                    agentWorkTracker.ResetAgentWorkCountForGift(giftName, agentId);
                }
            }
        }

        /// <summary>Runs before FinishWorkSuccessfully to capture the current agent's ID.
        /// Prefix is required because GetProb() is called inside the original method body,
        /// and the GetProb patch needs to know which agent is currently working.</summary>
        /// <param name="__instance"></param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix([NotNull] UseSkill __instance)
        {
            try
            {
                Harmony_Patch.Instance.CurrentWorkingAgentId =
                    __instance.PatchBeforeFinishWorkSuccessfully();
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
                    Harmony_Patch.Instance.Config
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
            }
        }
        // ReSharper enable InconsistentNaming
    }
}
