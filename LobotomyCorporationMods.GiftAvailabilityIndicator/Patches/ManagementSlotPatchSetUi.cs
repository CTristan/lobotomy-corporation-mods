// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.Facades;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Patches
{
    [HarmonyPatch(typeof(ManagementSlot), nameof(ManagementSlot.SetUI))]
    public static class ManagementSlotPatchSetUi
    {
        public static void PatchAfterSetUi([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] string imagePath,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IFileManager fileManager = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            var imageName = instance.GetSlotName(testAdapter);
            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            if (!instance.AbnormalityHasGift())
            {
                instance.HideImageObject(imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
                return;
            }

            var giftSlot = instance.GetAbnormalityGiftSlot();
            var giftsInSameSlot = agent.HasGiftInPosition(giftSlot);
            if (giftsInSameSlot)
            {
                var giftId = instance.GetAbnormalityGiftId();
                if (agent.HasGift(giftId))
                {
                    instance.HideImageObject(imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
                }
                else
                {
                    instance.ShowAsReplacementGift(imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
                }
            }
            else
            {
                instance.ShowAsNewGift(imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
            }
        }

        /// <summary>Runs after initializing the management slot UI to add our own additional icon.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] ManagementSlot __instance,
            [NotNull] UnitModel agent)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));
                Guard.Against.Null(agent, nameof(agent));

                const string ImagePath = "Assets/gift.png";

                __instance.PatchAfterSetUi(agent, ImagePath);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
