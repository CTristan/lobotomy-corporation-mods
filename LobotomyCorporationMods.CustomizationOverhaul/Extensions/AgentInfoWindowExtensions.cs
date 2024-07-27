// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.UiComponents;
using UnityEngine;

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
                    Harmony_Patch.Instance.LoadPresetButton = new GameObject().AddComponent<LoadPresetButton>();
                }
                else
                {
                    Harmony_Patch.Instance.LoadPresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.LoadPresetButton.IsUnityNull())
                {
                    return;
                }

                Harmony_Patch.Instance.LoadPresetButton.gameObject.SetActive(false);
            }
        }

        internal static void CreateSavePresetButton(this AgentInfoWindow agentInfoWindow)
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (Harmony_Patch.Instance.SavePresetButton.IsUnityNull())
                {
                    Harmony_Patch.Instance.SavePresetButton = new GameObject().AddComponent<SavePresetButton>();
                }
                else
                {
                    Harmony_Patch.Instance.SavePresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (Harmony_Patch.Instance.SavePresetButton.IsUnityNull())
                {
                    return;
                }

                Harmony_Patch.Instance.SavePresetButton.gameObject.SetActive(false);
            }
        }
    }
}
