// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), nameof(CreatureEquipmentMakeInfo.GetProb))]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        /// <summary>Increases the chance of getting the gift based on total work count.</summary>
        /// <param name="instance">The instance of CreatureEquipmentMakeInfo.</param>
        /// <param name="probability">The original probability value.</param>
        /// <param name="agentWorkTracker">The agent work tracker.</param>
        /// <returns>The modified probability value.</returns>
        public static float PatchAfterGetProb([NotNull] this CreatureEquipmentMakeInfo instance,
            float probability,
            [NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var giftName = instance.GetAbnormalityGiftName();
            probability = ModifyProbabilityIfGiftNameIsValid(probability, agentWorkTracker, giftName);

            return ValidateProbability(probability);
        }

        /// <summary>Modifies the probability value if the gift name is valid.</summary>
        /// <param name="probability">The original probability value.</param>
        /// <param name="agentWorkTracker">The agent work tracker.</param>
        /// <param name="giftName">The name of the gift.</param>
        /// <returns>The modified probability value.</returns>
        private static float ModifyProbabilityIfGiftNameIsValid(float probability,
            IAgentWorkTracker agentWorkTracker,
            [CanBeNull] string giftName)
        {
            if (string.IsNullOrEmpty(giftName))
            {
                return probability;
            }

            var probabilityBonus = agentWorkTracker.GetLastAgentWorkCountByGift(giftName) / 100f;
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
        public static void Postfix([NotNull] CreatureEquipmentMakeInfo __instance,
            ref float __result)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                __result = PatchAfterGetProb(__instance, __result, Harmony_Patch.Instance.AgentWorkTracker);
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
