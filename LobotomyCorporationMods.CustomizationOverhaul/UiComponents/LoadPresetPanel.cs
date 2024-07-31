// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
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
                this.SetImage(Application.dataPath + "/Managed/BaseMod/Image/Back.png");
                this.SetSize(PresetConstants.LoadPresetPanelSizeX, PresetConstants.LoadPresetPanelSizeY);
                this.SetPosition(PresetConstants.LoadPresetPanelPositionX, PresetConstants.LoadPresetPanelPositionY);

                UiPresetList = gameObject.AddComponent<UiPresetList>();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);
                throw;
            }
        }

        public void Update()
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

        public new void OnEnable()
        {
            base.OnEnable();

            UiPresetList.UpdatePage();
        }
    }
}
