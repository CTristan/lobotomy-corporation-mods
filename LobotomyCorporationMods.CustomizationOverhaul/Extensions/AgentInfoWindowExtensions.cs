// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.Extensions
{
    internal static class AgentInfoWindowExtensions
    {
        internal static void CreateLoadPresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.LoadPresetButton.IsUnityNull())
                {
                    Harmony_Patch.Instance.LoadPresetButton = new LoadPresetButton();
                }
                else
                {
                    Harmony_Patch.Instance.LoadPresetButton.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.LoadPresetButton.IsUnityNull())
                {
                    return;
                }

                Harmony_Patch.Instance.LoadPresetButton.SetActive(false);
            }
        }

        internal static void CreateSavePresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.SavePresetButton.IsUnityNull())
                {
                    Harmony_Patch.Instance.SavePresetButton = new SavePresetButton();
                }
                else
                {
                    Harmony_Patch.Instance.SavePresetButton.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.SavePresetButton.IsUnityNull())
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButton.SetActive(false);
            }
        }
    }
}
