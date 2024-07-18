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
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;

#endregion

namespace LobotomyCorporationMods.CustomizationOverhaul.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.OnSetNametext))]
    // ReSharper disable once InconsistentNaming
    public static class AppearanceUIPatchOnSetNameText
    {
        public static void PatchAfterOnSetNameText([NotNull] this AppearanceUI instance,
            IFileManager fileManager = null,
            IPresetLoader presetLoader = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            presetLoader = presetLoader.EnsureNotNullWithMethod(() => new PresetLoader(fileManager));

            var agentName = instance.NameInput.text;
            if (!presetLoader.HasPreset(agentName))
            {
                return;
            }

            instance.copied.isCustomName = true;
            instance.copied.CustomName = agentName;
            instance.copied.isUniqueCredit = false;
            var loadedAgentData = presetLoader.LoadPreset(agentName);

            instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
            instance.SetAppearanceSprite(loadedAgentData);
            instance.SetCreditControl(true);

            Harmony_Patch.Instance.PresetSaver.UpdateSavePresetButtonText(agentName, loadedAgentData.appearance);
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static void Postfix([NotNull] AppearanceUI __instance)
        {
            try
            {
                __instance.PatchAfterOnSetNameText();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
