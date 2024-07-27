// SPDX-License-Identifier: MIT

using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public sealed class DeletePresetSlotConfirmationPanel : Image
    {
        internal void Initialize(string presetName)
        {
            var text = gameObject.AddComponent<Text>();
            text.text = presetName;
        }
    }
}
