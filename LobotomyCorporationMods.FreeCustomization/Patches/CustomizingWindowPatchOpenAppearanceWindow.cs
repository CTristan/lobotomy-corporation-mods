// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using UnityEngine;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class CustomizingWindowPatchOpenAppearanceWindow
    {
        /// <summary>
        ///     Runs after opening the Appearance Window to make sure the IsCustomAppearance field is false, which is used by all
        ///     of the private methods to check for increasing the cost of custom agents.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static void Postfix(CustomizingWindow __instance)
        {
            try
            {
                if (!__instance.IsNullUnityObject())
                {
                    __instance.CurrentData.isCustomAppearance = false;
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
