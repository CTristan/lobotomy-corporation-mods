// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.UpdatePortrait))]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    public static class AppearanceUIPatchUpdatePortrait
    {
        // ReSharper disable once IdentifierTypo
        public static void PatchUpdatePortrait([NotNull] this AppearanceUI instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            var currentAgentName = instance.NameInput.text;
            if (string.IsNullOrEmpty(currentAgentName))
            {
                currentAgentName = instance.copied.agentName.GetName();
            }

            Harmony_Patch.Instance.PresetSaver.UpdateSavePresetButtonText(currentAgentName, instance.copied.appearance);
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AppearanceUI __instance)
        {
            try
            {
                __instance.PatchUpdatePortrait();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
