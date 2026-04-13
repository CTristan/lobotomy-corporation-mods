// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Harmony;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Constants;

#endregion

namespace LobotomyCorporationMods.NotifyWhenAgentReceivesGift.Patches
{
    [HarmonyPatch(typeof(UnitModel), nameof(UnitModel.AttachEGOgift))]
    public static class UnitModelPatchAttachEgoGift
    {
        public static void PatchBeforeAttachEgoGift(
            this UnitModel instance,
            EquipmentModel gift,
            INoticeInternals noticeInternals = null
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            ThrowHelper.ThrowIfNull(gift, nameof(gift));

            // Some gifts are in special slots that don't show up in an agent's gift window and are used for abnormality effects.
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

            // If we already have this gift equipped, we don't want to send an unnecessary notification
            if (instance.HasGift(gift.metaInfo.id))
            {
                return;
            }

            // Send notification that the agent acquired the gift
            var notificationMessage = LocalizationIds.LogMessage.GetLocalized();
            var agentColorCode = LocalizationIds.AgentColorCode.GetLocalized();
            var giftColorCode = LocalizationIds.GiftColorCode.GetLocalized();
            var agentColoredName = $"<color={agentColorCode}>{instance.GetUnitName()}</color>";
            var giftColoredName = $"<color={giftColorCode}>{gift.metaInfo.Name}</color>";
            var message = string.Format(
                CultureInfo.InvariantCulture,
                notificationMessage,
                agentColoredName,
                giftColoredName
            );
            instance.SendMessage(message, noticeInternals);
        }

        /// <summary>Needs to run before the method because we need to check ahead of time if the agent already has the gift or has another gift in the same position that is locked.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Prefix(UnitModel __instance, EGOgiftModel gift)
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
