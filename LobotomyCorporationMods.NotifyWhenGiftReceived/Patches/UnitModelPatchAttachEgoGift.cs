// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Patches
{
    // ReSharper disable once StringLiteralTypo
    [HarmonyPatch(typeof(UnitModel), "AttachEGOgift")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class UnitModelPatchAttachEgoGift
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix([NotNull] UnitModel __instance, [NotNull] EGOgiftModel gift)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                Guard.Against.Null(gift, nameof(gift));

                // If we already have this gift equipped we don't want to send an unnecessary notification
                if (__instance.HasGiftEquipped(gift.metaInfo.id))
                {
                    return;
                }

                // Check if the gift's position already has a locked gift
                if (__instance.PositionHasLockedGift(gift))
                {
                    return;
                }

                // Send notification that the agent acquired the gift
                var message = $"<color=#66bfcd>{__instance.GetUnitName()}</color> has received the gift <color=#84bd36>{gift.metaInfo.Name}</color>.";
                Notice.instance.Send(NoticeName.AddSystemLog, message);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
