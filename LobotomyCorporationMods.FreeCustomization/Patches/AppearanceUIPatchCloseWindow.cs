// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using Harmony;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), "CloseWindow")]
    public static class AppearanceUIPatchCloseWindow
    {
        /// <summary>
        ///     Runs before the Close Window function of the AppearanceUI runs to verify if we actually want to close the window.
        ///     The only reason we do this is because there's a hardcoded call to a private method (CustomizingWindow.Start()) that
        ///     closes the appearance window after the first agent window is generated.
        /// </summary>
        public static bool Prefix(AppearanceUI? __instance)
        {
            try
            {
                return __instance is not null ? __instance.closeAction is not null : throw new ArgumentNullException(nameof(__instance));
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteToLog(ex);

                throw;
            }
        }
    }
}
