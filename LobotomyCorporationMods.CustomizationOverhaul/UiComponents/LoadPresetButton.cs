// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public sealed class LoadPresetButton : AgentInfoWindowButton
    {
        public new void Awake()
        {
            try
            {
                base.Awake();

                transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                onClick.AddListener(ActionsOnClick);

                var imagePath = Harmony_Patch.Instance.FileManager.GetFile(PresetConstants.PresetPanelImagePath);
                SetButtonImage(imagePath);
                image.SetSize(PresetConstants.ButtonSizeX, PresetConstants.ButtonSizeY);
                image.SetPosition(PresetConstants.LoadPresetButtonPositionX, PresetConstants.LoadPresetButtonPositionY);

                Text.text = LocalizationIds.LoadPresetIconText.GetLocalized();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        private static void ActionsOnClick()
        {
            if (Harmony_Patch.Instance.LoadPresetPanel.IsUnityNull())
            {
                Harmony_Patch.Instance.LoadPresetPanel = new GameObject().AddComponent<LoadPresetPanel>();
                return;
            }

            Harmony_Patch.Instance.LoadPresetPanel.gameObject.SetActive(!Harmony_Patch.Instance.LoadPresetPanel.isActiveAndEnabled);
        }
    }
}
