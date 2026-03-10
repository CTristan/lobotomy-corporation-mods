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
using LobotomyCorporationMods.Common.ParameterObjects;
using LobotomyCorporationMods.GiftAlertIcon.Extensions;

#endregion

namespace LobotomyCorporationMods.GiftAlertIcon.Patches
{
    [HarmonyPatch(typeof(ManagementSlot), nameof(ManagementSlot.SetUI))]
    public static class ManagementSlotPatchSetUi
    {
        public static void PatchAfterSetUi([NotNull] this ManagementSlot instance,
            [NotNull] UnitModel agent,
            [NotNull] string imagePath,
            [CanBeNull] IFileManager fileManager = null,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            _ = Guard.Against.Null(instance, nameof(instance));
            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            testAdapterParameters = testAdapterParameters.EnsureNotNullWithMethod(() => new OptionalTestAdapterParameters());

            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;

            var imageId = instance.GetSlotName(testAdapterParameters.ManagementSlotTestAdapter);
            ImageParameters imageProperties = new ImageParameters
            {
                ImageId = imageId,
                ImageFilePath = imagePath,
                LocalPositionX = LocalPositionX,
                LocalPositionY = LocalPositionY,
                LocalPositionZ = LocalPositionZ,
                LocalScaleX = LocalScaleX,
                LocalScaleY = LocalScaleY,
            };

            instance.UpdateGiftIcon(agent, imageProperties, fileManager, testAdapterParameters);
        }

        public static void PostfixWithLogging(Func<ManagementSlot> getManagementSlot,
            [NotNull] UnitModel agent,
            Func<IFileManager> getFileManager)
        {
            try
            {
                _ = Guard.Against.Null(agent, nameof(agent));

                const string ImagePath = "Assets/gift.png";

                getManagementSlot().PatchAfterSetUi(agent, ImagePath, getFileManager());
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }

        /// <summary>Runs after initializing the management slot UI to add our own additional icon.</summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] ManagementSlot __instance,
            [NotNull] UnitModel agent)
        {
            PostfixWithLogging(() => __instance, agent, () => Harmony_Patch.Instance.FileManager);
        }
    }
}
