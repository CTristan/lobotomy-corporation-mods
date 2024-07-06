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
using LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Patches
{
    [HarmonyPatch(typeof(ManagementSlot), nameof(ManagementSlot.SetUI))]
    public static class ManagementSlotPatchSetUi
    {
        public static void PatchAfterSetUi([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [CanBeNull] IFileManager fileManager = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            var imageName = instance.name;
            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            if (!instance.AbnormalityHasGift())
            {
                instance.HideImageObject(imageName, fileManager);
                return;
            }

            var giftSlot = instance.GetAbnormalityGiftSlot();
            var giftsInSameSlot = agent.HasGiftInPosition(giftSlot);
            if (giftsInSameSlot)
            {
                var giftId = instance.GetAbnormalityGiftId();
                if (agent.HasGift(giftId))
                {
                    instance.HideImageObject(imageName, fileManager);
                }
                else
                {
                    instance.ShowAsReplacementGift(imageName, fileManager);
                }
            }
            else
            {
                instance.ShowAsNewGift(imageName, fileManager);
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

                __instance.PatchAfterSetUi(agent);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
