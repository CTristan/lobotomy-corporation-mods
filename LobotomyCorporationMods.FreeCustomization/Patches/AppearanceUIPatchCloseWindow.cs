// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.CloseWindow))]
    public static class AppearanceUIPatchCloseWindow
    {
        public static bool PatchBeforeCloseWindow([NotNull] this AppearanceUI instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            return !(instance.closeAction is null);
        }

        /// <summary>
        ///     Runs before the Close Window function of the AppearanceUI runs to verify if we actually want to close the window.
        ///     The only reason we do this is because there's a hardcoded call to a private method (CustomizingWindow.Start()) that
        ///     closes the appearance window after the first agent window is generated.
        /// </summary>
        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage]
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
