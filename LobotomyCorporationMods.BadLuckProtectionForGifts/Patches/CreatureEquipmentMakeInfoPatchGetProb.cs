// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), nameof(CreatureEquipmentMakeInfo.GetProb))]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] CreatureEquipmentMakeInfo __instance,
            ref float __result)
        {
            try
            {
                __result = __instance.PatchAfterGetProb(__result, Harmony_Patch.Instance.AgentWorkTracker);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
        // ReSharper enable InconsistentNaming

        public static float PatchAfterGetProb([NotNull] this CreatureEquipmentMakeInfo instance,
            float probability,
            [NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var giftName = instance.equipTypeInfo?.Name;

            // If creature has no gift then giftName will be null
            if (giftName.IsNull())
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
    }
}
