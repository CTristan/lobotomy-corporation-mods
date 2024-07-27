// SPDX-License-Identifier: MIT

using System;
using Customizing;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents.BaseComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public class PresetSlotButton : AgentInfoWindowButton
    {
        public new void Awake()
        {
            try
            {
                base.Awake();

                var fileManager = Harmony_Patch.Instance.FileManager;
                var imagePath = fileManager.GetFile("Assets/preset-panel.png");
                SetButtonImage(imagePath);
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        public void Setup(int buttonNum,
            string presetName)
        {
            try
            {
                Text.text = presetName;

                image.SetPosition(0.0f, PresetConstants.LoadPresetPanelPositionY - buttonNum * Height);

                onClick.AddListener(delegate
                {
                    var loadedAgentData = Harmony_Patch.Instance.PresetLoader.LoadPreset(presetName);

                    var instance = CustomizingWindow.CurrentWindow.appearanceUI;
                    instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
                    instance.SetAppearanceSprite(loadedAgentData);
                    instance.SetCreditControl(true);

                    Harmony_Patch.Instance.PresetSaver.UpdateSavePresetButtonText(presetName, loadedAgentData.appearance);
                });
            }
            catch (Exception e)
            {
                Harmony_Patch.Instance.Logger.WriteException(e);

                throw;
            }
        }
    }
}
