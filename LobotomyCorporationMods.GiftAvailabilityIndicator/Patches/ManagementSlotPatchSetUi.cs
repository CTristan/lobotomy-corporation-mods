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
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions;
using UnityEngine;
using UnityEngine.UI;

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
        private static readonly List<IGameObjectAdapter> s_slotImages = new();

        /// <summary>
        ///     Runs after initializing the management slot UI to add our own additional icon.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(ManagementSlot __instance, UnitModel agent)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                if (agent is null)
                {
                    throw new ArgumentNullException(nameof(agent));
                }

                var componentAdapter = new ComponentAdapter { GameObject = __instance };

                var commandWindow = CommandWindow.CommandWindow.CurrentWindow;
                var gameObjectAdapter = new GameObjectAdapter { GameObject = new GameObject() };
                var fileManager = Harmony_Patch.Instance.PublicFileManager;
                var texture2DAdapter = new Texture2DAdapter { GameObject = new Texture2D(2, 2) };
                var spriteAdapter = new SpriteAdapter();
                var imageAdapter = new ImageAdapter();

                componentAdapter.PatchAfterSetUi(agent, commandWindow, gameObjectAdapter, fileManager, texture2DAdapter, spriteAdapter, imageAdapter);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        public static void PatchAfterSetUi(this IComponentAdapter instance, UnitModel agent, CommandWindow.CommandWindow commandWindow, IGameObjectAdapter imageGameObject, IFileManager fileManager,
            ITexture2DAdapter texture2DAdapter, ISpriteAdapter spriteAdapter, IImageAdapter imageAdapter)
        {
            var abnormalityGift = commandWindow.GetCreatureGiftIfExists();

            if (abnormalityGift != null)
            {
                var slotNumber = instance.Name switch
                {
                    SlotOneName => 0,
                    SlotTwoName => 1,
                    SlotThreeName => 2,
                    SlotFourName => 3,
                    SlotFiveName => 4,
                    _ => throw new InvalidOperationException(instance.Name + " is not a valid slot name")
                };

                if (s_slotImages.Count < slotNumber + 1)
                {
                    s_slotImages.Add(instance.CreateImageGameObject(imageGameObject, fileManager, texture2DAdapter, spriteAdapter, imageAdapter));
                }

                if (s_slotImages[slotNumber].IsUnityNull())
                {
                    s_slotImages[slotNumber] = instance.CreateImageGameObject(imageGameObject, fileManager, texture2DAdapter, spriteAdapter, imageAdapter);
                }

                var image = s_slotImages[slotNumber].GetComponent<Image>();

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
