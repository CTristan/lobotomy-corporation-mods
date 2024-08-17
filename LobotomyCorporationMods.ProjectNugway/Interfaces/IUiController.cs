// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.ProjectNugway.UiComponents;

namespace LobotomyCorporationMods.ProjectNugway.Interfaces
{
    public interface IUiController
    {
        ButtonWithText LoadPresetButton { get; }
        LoadPresetPanel LoadPresetPanel { get; }
        ButtonWithText SavePresetButton { get; }

        void DisplayLoadPresetButton();
        void DisplayLoadPresetPanel();
        void DisplaySavePresetButton();
        void DisableAllCustomUiComponents();

        void UpdateSavePresetButtonText(string agentName,
            Appearance appearance);
    }
}
