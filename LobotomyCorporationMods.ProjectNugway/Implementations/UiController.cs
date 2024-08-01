// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.UiComponents;
using UnityEngine;

namespace LobotomyCorporationMods.ProjectNugway.Implementations
{
    internal sealed class UiController : IUiController
    {
        public LoadPresetButton LoadPresetButton { get; private set; }
        public LoadPresetPanel LoadPresetPanel { get; private set; }
        public SavePresetButton SavePresetButton { get; private set; }

        public void DisplayLoadPresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (LoadPresetButton == null)
                {
                    LoadPresetButton = new GameObject().AddComponent<LoadPresetButton>();
                }
                else
                {
                    LoadPresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (LoadPresetButton == null)
                {
                    return;
                }

                LoadPresetButton.gameObject.SetActive(false);
            }
        }

        public void DisplaySavePresetButton()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (SavePresetButton == null)
                {
                    SavePresetButton = new GameObject().AddComponent<SavePresetButton>();
                }
                else
                {
                    SavePresetButton.gameObject.SetActive(true);
                }
            }
            else
            {
                if (SavePresetButton == null)
                {
                    return;
                }

                SavePresetButton.gameObject.SetActive(false);
            }
        }

        public void DisableAllCustomUiComponents()
        {
            LoadPresetButton.gameObject.SetActive(false);

            SavePresetButton.gameObject.SetActive(false);

            LoadPresetPanel.gameObject.SetActive(false);
        }

        public void DisplayLoadPresetPanel()
        {
            if (!GameManager.currentGameManager.ManageStarted)
            {
                if (LoadPresetPanel == null)
                {
                    LoadPresetPanel = new GameObject().AddComponent<LoadPresetPanel>();
                }
                else
                {
                    LoadPresetPanel.gameObject.SetActive(true);
                }
            }
            else
            {
                if (LoadPresetPanel == null)
                {
                    return;
                }

                LoadPresetPanel.gameObject.SetActive(false);
            }
        }
    }
}
