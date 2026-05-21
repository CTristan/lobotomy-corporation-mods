// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class LoadPresetPanel : Image
    {
        private UiPresetList UiPresetList { get; set; }

        public new void Awake()
        {
            try
            {
                base.Awake();

                gameObject.SetActive(true);
                transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                sprite = SpriteLoader.LoadSpriteFromFile(
                    Application.dataPath + "/Managed/BaseMod/Image/Back.png"
                );
                rectTransform.sizeDelta = new Vector2(
                    UiComponentConstants.LoadPresetPanelSizeX,
                    UiComponentConstants.LoadPresetPanelSizeY
                );
                rectTransform.anchoredPosition = new Vector2(
                    UiComponentConstants.LoadPresetPanelPositionX,
                    UiComponentConstants.LoadPresetPanelPositionY
                );

                UiPresetList = gameObject.AddComponent<UiPresetList>();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        public void Update()
        {
            try
            {
                HandleScrollWheelInput();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        public new void OnEnable()
        {
            try
            {
                base.OnEnable();

                UiPresetList.UpdatePage();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);

                throw;
            }
        }

        private void HandleScrollWheelInput()
        {
            var scrollData = Input.GetAxis("Mouse ScrollWheel");

            if (scrollData > 0.0f)
            {
                UiPresetList.OnClickUpButton();
            }
            else if (scrollData < 0.0f)
            {
                UiPresetList.OnClickDownButton();
            }
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                // Scroll wheel is not being used
            }
        }
    }
}
