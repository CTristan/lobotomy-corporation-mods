// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Constants;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches
{
    [HarmonyPatch(typeof(UnitModel), nameof(UnitModel.AttachEGOgift))]
    public static class UnitModelPatchAttachEgoGift
    {
        public static void PatchBeforeAttachEgoGift(this UnitModel instance,
            EquipmentModel gift,
            INoticeTestAdapter noticeTestAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(gift, nameof(gift));

            // Some gifts are in special slots that don't show up in an agent's gift window and are used for abnormality effects
            // For example, Snow Queen's icicle
            if (!gift.IsInValidSlot())
            {
                return;
            }

            // Check if the gift's position already has a locked gift
            if (instance.PositionHasLockedGift(gift))
            {
                return;
            }

            // If we already have this gift equipped we don't want to send an unnecessary notification
            if (instance.HasGift(gift.metaInfo.id))
            {
                return;
            }

            // Send notification that the agent acquired the gift
            var notificationMessage = LocalizeIds.LogMessage.GetLocalized();
            var message = $"<color=#66bfcd>{instance.GetUnitName()}</color>{notificationMessage}<color=#84bd36>{gift.metaInfo.Name}</color>.";
            instance.SendMessage(message, noticeTestAdapter);
        }

        /// <summary>Needs to run before the method because we need to check ahead of time if the agent already has the gift or has another gift in the same position that is locked.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix(UnitModel __instance,
            EGOgiftModel gift)
        {
            try
            {
                __instance.PatchBeforeAttachEgoGift(gift);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
