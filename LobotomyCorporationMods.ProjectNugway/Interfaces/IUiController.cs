// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.ProjectNugway.UiComponents;

namespace LobotomyCorporationMods.ProjectNugway.Interfaces
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
