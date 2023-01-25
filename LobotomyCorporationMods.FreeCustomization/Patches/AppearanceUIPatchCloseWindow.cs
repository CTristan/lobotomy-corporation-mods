// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), "CloseWindow")]
    [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
    public static class AppearanceUIPatchCloseWindow
    {
        /// <summary>
        ///     Runs before the Close Window function of the AppearanceUI runs to verify if we actually want to close the window.
        ///     The only reason we do this is because there's a hardcoded call to a private method (CustomizingWindow.Start()) that
        ///     closes the appearance window after the first agent window is generated.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static bool Prefix([NotNull] AppearanceUI __instance)
        {
            try
            {
                Guard.Against.Null(__instance, nameof(__instance));

                return __instance.closeAction != null;
            }
            catch (Exception ex)
            {
                // Null argument exception only comes up during testing due to Unity operator overloading.
                // https://github.com/JetBrains/resharper-unity/wiki/Possible-unintended-bypass-of-lifetime-check-of-underlying-Unity-engine-object
                if (ex is ArgumentNullException)
                {
                    return true;
                }

                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
