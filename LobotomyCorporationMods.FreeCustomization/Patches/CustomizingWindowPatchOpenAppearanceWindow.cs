// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using LobotomyCorporationMods.Common.Attributes;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(CustomizingWindow), "OpenAppearanceWindow")]
    public static class CustomizingWindowPatchOpenAppearanceWindow
    {
        /// <summary>
        ///     Runs after opening the Appearance Window to make sure the IsCustomAppearance field is false, which is used by all
        ///     of the private methods to check for increasing the cost of custom agents.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
        public static void Postfix(CustomizingWindow __instance)
        {
            try
            {
                __instance.PatchAfterOpenAppearanceWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }

        public static void PatchAfterOpenAppearanceWindow(this CustomizingWindow instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            instance.CurrentData.isCustomAppearance = false;
        }
    }
}
