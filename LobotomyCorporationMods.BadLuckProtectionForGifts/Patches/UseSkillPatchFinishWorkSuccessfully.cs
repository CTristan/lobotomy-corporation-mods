// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common.Attributes;
using LobotomyCorporation.Mods.Common.Constants;
using LobotomyCorporation.Mods.Common.Extensions;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporation.Mods.Common.Implementations.Facades;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(UseSkill), PrivateMethods.UseSkill.FinishWorkSuccessfully)]
    public static class UseSkillPatchFinishWorkSuccessfully
    {
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

        /// <summary>Runs after an agent finishes working with an abnormality to increment their work count.</summary>
        /// <param name="__instance"></param>
        // ReSharper disable InconsistentNaming
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
        }
        // ReSharper enable InconsistentNaming
    }
}
