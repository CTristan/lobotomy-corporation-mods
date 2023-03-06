// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandWindow;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Patches
{
    [HarmonyPatch(typeof(AgentSlot), "SetUI")]
    public static class AgentSlotPatchSetUi
    {
        private const string SlotFiveName = "Slot (8)";
        private const string SlotFourName = "Slot (7)";
        private const string SlotOneName = "Slot";
        private const string SlotThreeName = "Slot (6)";
        private const string SlotTwoName = "Slot (5)";
        private static Image? s_slotFiveImage;
        private static Image? s_slotFourImage;
        private static Image? s_slotOneImage;
        private static Image? s_slotThreeImage;
        private static Image? s_slotTwoImage; // ReSharper disable InconsistentNaming

        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Prefix(AgentSlot __instance, AgentModel agent)
        {
            try
            {
                __instance.PatchBeforeSetUi(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }

        public static void PatchBeforeSetUi(this AgentSlot instance, UnitModel agent)
        {
            Image image;

            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            if (commandWindow is not null && commandWindow.CurrentWindowType == CommandType.Management)
            {
                switch (instance.name)
                {
                    case SlotOneName:
                        s_slotOneImage ??= instance.CreateGiftAvailabilityImage();
                        image = s_slotOneImage;

                        break;
                    case SlotTwoName:
                        s_slotTwoImage ??= instance.CreateGiftAvailabilityImage();
                        image = s_slotTwoImage;

                        break;
                    case SlotThreeName:
                        s_slotThreeImage ??= instance.CreateGiftAvailabilityImage();
                        image = s_slotThreeImage;

                        break;
                    case SlotFourName:
                        s_slotFourImage ??= instance.CreateGiftAvailabilityImage();
                        image = s_slotFourImage;

                        break;
                    case SlotFiveName:
                        s_slotFiveImage ??= instance.CreateGiftAvailabilityImage();
                        image = s_slotFiveImage;

                        break;
                    default:
                        throw new InvalidOperationException(instance.name + " is not a valid slot name");
                }

                var abnormalityGift = commandWindow.GetCreatureGiftIfExists();

                if (abnormalityGift is not null)
                {
                    var giftName = abnormalityGift.equipTypeInfo.Name;
                    var giftSlot = abnormalityGift.equipTypeInfo.attachPos;

                    var agentGifts = agent.GetEquippedGifts();
                    var giftsInSameSlot = agentGifts.Where(model => model.metaInfo.attachPos == giftSlot).ToList();

                    if (giftsInSameSlot.Any())
                    {
                        image.color = giftsInSameSlot.Any(model => model.metaInfo.Name == giftName) ? Color.clear : Color.gray;
                    }
                    else
                    {
                        image.color = Color.green;
                    }
                }
            }
        }
    }
}
