// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.GiftAlertIcon.Extensions;

#endregion

namespace LobotomyCorporationMods.GiftAlertIcon.Patches
{
    [HarmonyPatch(typeof(ManagementSlot), nameof(ManagementSlot.SetUI))]
    public static class ManagementSlotPatchSetUi
    {
        public static void PatchAfterSetUi(
            [NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] string imagePath,
            [CanBeNull] IFileManager fileManager = null,
            [CanBeNull] OptionalOverrides optionalOverrides = null
        )
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));
            fileManager = fileManager.OrCreate(() => Harmony_Patch.Instance.FileManager);
            optionalOverrides = optionalOverrides.OrCreate(() => new OptionalOverrides());

            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;

            var imageId = instance.GetSlotName(optionalOverrides.ManagementSlotInternals);
            var imageProperties = new ImageParameters
            {
                ImageId = imageId,
                ImageFilePath = imagePath,
                LocalPositionX = LocalPositionX,
                LocalPositionY = LocalPositionY,
                LocalPositionZ = LocalPositionZ,
                LocalScaleX = LocalScaleX,
                LocalScaleY = LocalScaleY,
            };

            instance.UpdateGiftIcon(agent, imageProperties, fileManager, optionalOverrides);
        }

        /// <summary>Runs after initializing the management slot UI to add our own additional icon.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] ManagementSlot __instance, [NotNull] UnitModel agent)
        {
            try
            {
                ThrowHelper.ThrowIfNull(__instance, nameof(__instance));
                ThrowHelper.ThrowIfNull(agent, nameof(agent));

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
