// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.ProjectNugway.Constants;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.UiComponents.BaseComponents;

namespace LobotomyCorporationMods.ProjectNugway.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class LoadPresetButton : AgentInfoWindowButton
    {
        public new void Awake()
        {
            try
            {
                base.Awake();

                transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                onClick.AddListener(() => ActionsOnClick(Harmony_Patch.Instance.UiController));

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath);
                SetButtonImage(imagePath);
                image.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                image.SetPosition(PresetConstants.LoadPresetButtonPositionX, PresetConstants.LoadPresetButtonPositionY);

                Text.text = LocalizationIds.LoadPresetIconText.GetLocalized();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

                throw;
            }
        }

        private static void ActionsOnClick([NotNull] IUiController uiController)
        {
            // Make sure that are presets are the most current whenever we click the load preset button
            Harmony_Patch.Instance.PresetLoader.ReloadPresetsFromFiles();

            if (uiController.LoadPresetPanel == null)
            {
                uiController.DisplayLoadPresetPanel();

                return;
            }

            uiController.LoadPresetPanel.gameObject.SetActive(!uiController.LoadPresetPanel.isActiveAndEnabled);
        }
    }
}
