// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Extensions;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches
{
    [HarmonyPatch(typeof(UnitModel), nameof(UnitModel.AttachEGOgift))]
    public static class UnitModelPatchAttachEgoGift
    {
        public static void PatchBeforeAttachEgoGift(UnitModel instance, EquipmentModel gift,
            INoticeAdapter noticeAdapter)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(gift, nameof(gift));
            Guard.Against.Null(noticeAdapter, nameof(noticeAdapter));

            // If we already have this gift equipped we don't want to send an unnecessary notification
            if (instance.HasGiftEquipped(gift.metaInfo.id))
            {
                return;
            }

            // Check if the gift's position already has a locked gift
            if (instance.PositionHasLockedGift(gift))
            {
                return;
            }

            // Send notification that the agent acquired the gift
            var message =
                $"<color=#66bfcd>{instance.GetUnitName()}</color> has received the gift <color=#84bd36>{gift.metaInfo.Name}</color>.";
            noticeAdapter.Send(NoticeName.AddSystemLog, message);
        }

        /// <summary>
        ///     Needs to run before the method because we need to check ahead of time if the agent already has the gift or has
        ///     another gift in the same position that is locked.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Prefix(UnitModel __instance, EGOgiftModel gift)
        {
            try
            {
                var noticeAdapter = new NoticeAdapter { GameObject = Notice.instance };
                PatchBeforeAttachEgoGift(__instance, gift, noticeAdapter);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
