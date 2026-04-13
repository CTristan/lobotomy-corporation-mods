// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.CloseWindow))]
    public static class AppearanceUiPatchCloseWindow
    {
        public static bool PatchBeforeCloseWindow([NotNull] this AppearanceUI instance)
        {
            ThrowHelper.ThrowIfNull(instance, nameof(instance));

            return !instance.closeAction.IsNull();
        }

        /// <summary>
        ///     Runs before the Close Window function of the AppearanceUI runs to verify if we actually want to close the window. The only reason we do this is there's a hardcoded call
        ///     to a private method (CustomizingWindow.Start()) that closes the appearance window after the first agent window is generated.
        /// </summary>
        /// <param name="__instance">An instance of the AppearanceUI class.</param>
        /// <returns>True if the prefix method successfully executed, otherwise false.</returns>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static bool Prefix([NotNull] AppearanceUI __instance)
        {
            try
            {
                return __instance.PatchBeforeCloseWindow();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
