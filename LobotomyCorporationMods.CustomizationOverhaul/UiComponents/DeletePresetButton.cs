// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public class DeletePresetButton : UiButtonWithText
    {
        public new void Awake()
        {
            base.Awake();

            var fileManager = Harmony_Patch.Instance.FileManager;
            var imagePath = fileManager.GetFile("Assets/preset-panel-delete-icon.png");
            SetButtonImage(imagePath);
            image.SetPosition(PresetConstants.DeletePresetButtonPositionX, PresetConstants.DeletePresetButtonPositionY);
        }
    }
}
