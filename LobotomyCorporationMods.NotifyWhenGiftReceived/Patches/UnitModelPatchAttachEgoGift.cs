// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.NotifyWhenGiftReceived.Extensions;

namespace LobotomyCorporationMods.NotifyWhenGiftReceived.Patches
{
    // ReSharper disable once StringLiteralTypo
    [HarmonyPatch(typeof(UnitModel), "AttachEGOgift")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class UnitModelPatchAttachEgoGift
    {
        // ReSharper disable once InconsistentNaming
        public static void Prefix([NotNull] UnitModel __instance, [CanBeNull] EGOgiftModel gift)
        {
            // If we already have this gift equipped we don't want to send an unnecessary notification
            if (__instance.HasGiftEquipped(gift.metaInfo.id)) { return; }

            // Send notification that the agent acquired the gift
            var message = $"<color=#66bfcd>{__instance.GetUnitName()}</color> has received the gift <color=#84bd36>{gift.metaInfo.Name}</color>.";
            Notice.instance.Send("AddSystemLog", message);
        }
    }
}
