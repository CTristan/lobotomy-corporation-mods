// SPDX-License-Identifier: MIT

using Customizing;
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

        void UpdateSavePresetButtonText(string agentName,
            Appearance appearance);
    }
}
