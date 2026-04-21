// SPDX-License-Identifier: MIT

using Customizing;
using LobotomyCorporationMods.Common.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.Interfaces
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
