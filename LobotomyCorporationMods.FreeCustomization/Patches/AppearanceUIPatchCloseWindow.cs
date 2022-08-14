// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
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
            // If the instance is null then don't do any checks and let processing continue
            if (Guard.Against.IsNull(__instance))
            {
                return true;
            }

            try
            {
                return __instance.closeAction != null;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.FileManager.WriteToLog(ex);

                throw;
            }
        }
    }
}
