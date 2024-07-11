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
        public static void PatchAfterOnSetNameText([NotNull] this AppearanceUI instance,
            IFileManager fileManager = null,
            IPresetLoader presetLoader = null)
        {
            Guard.Against.Null(instance, nameof(instance));

            fileManager = fileManager.EnsureNotNullWithMethod(() => Harmony_Patch.Instance.FileManager);
            presetLoader = presetLoader.EnsureNotNullWithMethod(() => new PresetLoader(fileManager));

            var name = instance.NameInput.text;
            if (name.Equals(instance.original.agentName.GetName(), StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(name))
            {
                instance.copied.isCustomName = false;
            }
            else
            {
                instance.copied.isCustomName = true;
                instance.copied.CustomName = name;

                // // Should only be needed to save the existing unique agents
                // // Can delete these lines afterward if not needed
                // var uniqueCreditInfo = AgentNameList.instance.GetUniqueCreditInfo(name);
                // CustomizingWindow.CurrentWindow.GenUniqueSpriteSet(uniqueCreditInfo, ref instance.copied);

                if (!presetLoader.IsPreset(name))
                {
                    return;
                }

                presetLoader.LoadPreset(name);

                instance.SetCreditControl(true);
            }
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
