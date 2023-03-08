// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommandWindow;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Patches
{
    [HarmonyPatch(typeof(ManagementSlot), "SetUI")]
    public static class ManagementSlotPatchSetUi
    {
        private const string SlotFiveName = "Slot (8)";
        private const string SlotFourName = "Slot (7)";
        private const string SlotOneName = "Slot";
        private const string SlotThreeName = "Slot (6)";
        private const string SlotTwoName = "Slot (5)";
        private static readonly List<GameObject> SlotImages = new();

        [EntryPoint]
        [ExcludeFromCodeCoverage]
        // ReSharper disable InconsistentNaming
        public static void Prefix(ManagementSlot __instance, AgentModel agent)
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

        public static void PatchBeforeSetUi(this ManagementSlot instance, UnitModel agent)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            if (commandWindow is not null)
            {
                var slotNumber = instance.name switch
                {
                    SlotOneName => 0,
                    SlotTwoName => 1,
                    SlotThreeName => 2,
                    SlotFourName => 3,
                    SlotFiveName => 4,
                    _ => throw new InvalidOperationException(instance.name + " is not a valid slot name")
                };

                if (SlotImages.Count < slotNumber + 1)
                {
                    SlotImages.Add(instance.CreateGiftAvailabilityImage());
                }

                if (SlotImages[slotNumber] == null)
                {
                    SlotImages[slotNumber] = instance.CreateGiftAvailabilityImage();
                }

                var image = instance.GetGiftAvailabilityImage(SlotImages[slotNumber]);

                var abnormalityGift = commandWindow.GetCreatureGiftIfExists();

                if (abnormalityGift is not null)
                {
                    var giftName = abnormalityGift.equipTypeInfo.Name;
                    var giftSlot = abnormalityGift.equipTypeInfo.attachPos;

                    var agentGifts = agent.GetEquippedGifts();
                    var giftsInSameSlot = agentGifts.Where(model => model.metaInfo.attachPos == giftSlot).ToList();

                    if (giftsInSameSlot.Any())
                    {
                        if (giftsInSameSlot.Any(model => model.metaInfo.Name == giftName))
                        {
                            image.Hide();
                        }
                        else
                        {
                            image.ShowAsReplacementGift();
                        }
                    }
                    else
                    {
                        image.ShowAsNewGift();
                    }
                }
            }
        }
    }
}
