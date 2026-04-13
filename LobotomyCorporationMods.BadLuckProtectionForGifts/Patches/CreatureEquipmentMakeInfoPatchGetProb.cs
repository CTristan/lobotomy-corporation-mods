// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), nameof(CreatureEquipmentMakeInfo.GetProb))]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        /// <summary>Increases the chance of getting the gift based on total work count.</summary>
        /// <param name="instance">The instance of CreatureEquipmentMakeInfo.</param>
        /// <param name="probability">The original probability value.</param>
        /// <param name="agentWorkTracker">The agent work tracker.</param>
        /// <param name="config">The configuration settings.</param>
        /// <param name="currentAgentId">The current working agent's ID, or null if called from UI context.</param>
        /// <returns>The modified probability value.</returns>
        public static float PatchAfterGetProb(
            [NotNull] this CreatureEquipmentMakeInfo instance,
            float probability,
            [NotNull] IAgentWorkTracker agentWorkTracker,
            [NotNull] IBadLuckProtectionConfig config,
            long? currentAgentId
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(agentWorkTracker, nameof(agentWorkTracker));
            ThrowHelper.ThrowIfNull(config, nameof(config));

            // No agent context means this is a UI display call, not a gift roll
            if (!currentAgentId.HasValue)
            {
                return probability;
            }

            var giftName = instance.GetAbnormalityGiftName();
            probability = ModifyProbabilityIfGiftNameIsValid(
                probability,
                agentWorkTracker,
                config,
                giftName,
                currentAgentId.Value
            );

            return ValidateProbability(probability);
        }

        /// <summary>Modifies the probability value if the gift name is valid.</summary>
        private static float ModifyProbabilityIfGiftNameIsValid(
            float probability,
            IAgentWorkTracker agentWorkTracker,
            IBadLuckProtectionConfig config,
            [CanBeNull] string giftName,
            long agentId
        )
        {
            if (string.IsNullOrEmpty(giftName))
            {
                return probability;
            }

            var workCount = agentWorkTracker.GetAgentWorkCountByGift(giftName, agentId);
            var riskLevel = agentWorkTracker.GetRiskLevelByGift(giftName);
            var bonusPercentage = riskLevel.HasValue
                ? config.GetBonusPercentageForRiskLevel(riskLevel.Value)
                : 1.0f;

            var probabilityBonus = workCount * bonusPercentage / 100f;
            probability += probabilityBonus;

            return probability;
        }

        /// <summary>Prevents probabilities higher than 100% which could cause potential overflow.</summary>
        /// <param name="probability">The probability value to validate.</param>
        /// <returns>The validated probability value.</returns>
        private static float ValidateProbability(float probability)
        {
            return probability > 1f ? 1f : probability;
        }

        /// <summary>Runs after GetProb to add on additional chance based on our work tracking.</summary>
        /// <param name="__instance">The instance of the CreatureEquipmentMakeInfo class.</param>
        /// <param name="__result">The result of the original GetProb method.</param>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix(
            [NotNull] CreatureEquipmentMakeInfo __instance,
            ref float __result
        )
        {
            try
            {
                ThrowHelper.ThrowIfNull(__instance, nameof(__instance));

                __result = PatchAfterGetProb(
                    __instance,
                    __result,
                    Harmony_Patch.Instance.AgentWorkTracker,
                    Harmony_Patch.Instance.Config,
                    Harmony_Patch.Instance.CurrentWorkingAgentId
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
