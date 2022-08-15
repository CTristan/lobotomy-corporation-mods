// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

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
        public static void Postfix([NotNull] CustomizingWindow __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                __instance.CurrentData.isCustomAppearance = false;
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
