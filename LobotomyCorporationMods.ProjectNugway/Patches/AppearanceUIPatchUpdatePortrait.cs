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
using LobotomyCorporationMods.ProjectNugway.Interfaces;

#endregion

namespace LobotomyCorporationMods.ProjectNugway.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.UpdatePortrait))]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once IdentifierTypo
    public static class AppearanceUIPatchUpdatePortrait
    {
        // ReSharper disable once IdentifierTypo
        public static void PatchAfterUpdatePortrait([NotNull] this AppearanceUI instance,
            [NotNull] IUiController uiController)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(uiController, nameof(uiController));

            var currentAgentName = instance.NameInput.text;
            if (string.IsNullOrEmpty(currentAgentName))
            {
                currentAgentName = instance.copied.agentName.GetName();
            }

            var agentInfoWindow = AgentInfoWindow.currentWindow;
            if (agentInfoWindow.IsNull())
            {
                return;
            }

            uiController.DisplaySavePresetButton();
            uiController.UpdateSavePresetButtonText(currentAgentName, instance.copied.appearance);
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AppearanceUI __instance)
        {
            try
            {
                __instance.PatchAfterUpdatePortrait(Harmony_Patch.Instance.UiController);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.LogError(ex);

                throw;
            }
        }
    }
}
