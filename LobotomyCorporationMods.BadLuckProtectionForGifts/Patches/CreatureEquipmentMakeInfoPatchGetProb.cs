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
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Facades;

#endregion

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Patches
{
    [HarmonyPatch(typeof(CreatureEquipmentMakeInfo), nameof(CreatureEquipmentMakeInfo.GetProb))]
    public static class CreatureEquipmentMakeInfoPatchGetProb
    {
        public static float PatchAfterGetProb([NotNull] ICreatureEquipmentMakeInfoFacade facade,
            float probability,
            [NotNull] IAgentWorkTracker agentWorkTracker)
        {
            Guard.Against.Null(facade, nameof(facade));
            Guard.Against.Null(agentWorkTracker, nameof(agentWorkTracker));

            var giftName = facade.GiftName;

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
        } // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] CreatureEquipmentMakeInfo __instance,
            ref float __result)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                __result = PatchAfterGetProb(new CreatureEquipmentMakeInfoFacade(__instance), __result, Harmony_Patch.Instance.AgentWorkTracker);
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
