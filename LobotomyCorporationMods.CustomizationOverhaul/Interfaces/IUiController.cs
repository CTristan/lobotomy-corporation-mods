// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.Interfaces
{
    public interface IUiController
    {
        LoadPresetButton LoadPresetButton { get; }
        LoadPresetPanel LoadPresetPanel { get; }
        SavePresetButton SavePresetButton { get; }

        void DisplayLoadPresetButton();
        void DisplayLoadPresetPanel();
        void DisplaySavePresetButton();
        void DisableAllCustomUiComponents();
    }
}
