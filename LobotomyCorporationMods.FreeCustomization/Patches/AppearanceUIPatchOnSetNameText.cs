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
using LobotomyCorporationMods.FreeCustomization.Implementations;
using LobotomyCorporationMods.FreeCustomization.Interfaces;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Patches
{
    [HarmonyPatch(typeof(AppearanceUI), nameof(AppearanceUI.OnSetNametext))]
    public static class AppearanceUiPatchOnSetNameText
    {
        public static void PatchBeforeOnSetNameText([NotNull] this AppearanceUI instance,
            IFileManager fileManager = null,
            IPresetLoader presetLoader = null,
            IPresetSaver presetSaver = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            presetLoader = presetLoader.EnsureNotNullWithMethod(() => new PresetLoader(fileManager));
            presetSaver = presetSaver.EnsureNotNullWithMethod(() => new PresetSaver(fileManager));

            var name = instance.NameInput.text;
            if (name.Equals(instance.original.agentName.GetName(), StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(name))
            {
                instance.copied.isCustomName = false;
            }
            else
            {
                instance.copied.isCustomName = true;
                instance.copied.CustomName = name;

                // Should only be needed to save the existing unique agents
                // Can delete these lines afterward if not needed
                var uniqueCreditInfo = AgentNameList.instance.GetUniqueCreditInfo(name);
                CustomizingWindow.CurrentWindow.GenUniqueSpriteSet(uniqueCreditInfo, ref instance.copied);

                presetSaver.SavePreset(name, instance.original.appearance);

                if (!presetLoader.IsPreset(name))
                {
                    return;
                }

                instance.SetCreditControl(false);
            }
        }

        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        // ReSharper disable once InconsistentNaming
        public static bool Prefix([NotNull] AppearanceUI __instance)
        {
            try
            {
                __instance.PatchBeforeOnSetNameText();

                return true;
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
