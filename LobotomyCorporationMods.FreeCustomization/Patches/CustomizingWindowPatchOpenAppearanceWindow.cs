// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using Harmony;

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
        // ReSharper disable once InconsistentNaming
        public static void Postfix(CustomizingWindow? __instance)
        {
            try
            {
                if (__instance is null)
                {
                    throw new ArgumentNullException(nameof(__instance));
                }

                __instance.CurrentData.isCustomAppearance = false;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
