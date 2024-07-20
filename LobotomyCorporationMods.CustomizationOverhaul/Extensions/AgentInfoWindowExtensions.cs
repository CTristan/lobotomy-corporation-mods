// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;

namespace LobotomyCorporationMods.CustomizationOverhaul.Extensions
{
    internal static class AgentInfoWindowExtensions
    {
        internal static void DisableAllCustomUiComponents(this AgentInfoWindow agentInfoWindow)
        {
            if (Harmony_Patch.Instance.LoadPresetButton.IsNotNull())
            {
                Harmony_Patch.Instance.LoadPresetButton.SetActive(false);
            }

            if (Harmony_Patch.Instance.SavePresetButton.IsNotNull())
            {
                Harmony_Patch.Instance.SavePresetButton.SetActive(false);
            }

            if (Harmony_Patch.Instance.LoadPresetPanel.IsNotNull())
            {
                Harmony_Patch.Instance.LoadPresetPanel.SetActive(false);
            }
        }

        internal static void CreateLoadPresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.LoadPresetButton.IsNull())
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
                if (Harmony_Patch.Instance.LoadPresetButton == null)
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
                if (Harmony_Patch.Instance.SavePresetButton.IsNull())
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
                if (Harmony_Patch.Instance.SavePresetButton == null)
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButton.SetActive(false);
            }
        }
    }
}
