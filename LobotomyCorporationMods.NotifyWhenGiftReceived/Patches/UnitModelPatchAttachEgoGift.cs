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

                // TODO: Check for locked gift

                // Send notification that the agent acquired the gift
                var message = $"<color=#66bfcd>{__instance.GetUnitName()}</color> has received the gift <color=#84bd36>{gift.metaInfo.Name}</color>.";
                Notice.instance.Send("AddSystemLog", message);
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}