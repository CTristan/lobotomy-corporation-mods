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
        /// <summary>Increases the chance of getting the gift based on total work count</summary>
        public static float PatchAfterGetProb([NotNull] this CreatureEquipmentMakeInfo instance,
            float probability,
            [NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var giftName = instance.GetGift()?.Name ?? string.Empty;

            // If the abnormality has no gift then there's nothing to track
            if (string.IsNullOrEmpty(giftName))
            {
                return probability;
            }

            var probabilityBonus = agentWorkTracker.GetLastAgentWorkCountByGift(giftName) / 100f;
            probability += probabilityBonus;

            // Prevent potential overflow issues
            if (probability > 1f)
            {
                probability = 1f;
            }

            return probability;
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
