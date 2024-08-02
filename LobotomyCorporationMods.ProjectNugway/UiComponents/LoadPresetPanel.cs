// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.ProjectNugway.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.ProjectNugway.UiComponents
{
    [UiComponent]
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
            try
            {
                HandleScrollWheelInput();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.LogError(exception);

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
                Harmony_Patch.Instance.Logger.LogError(exception);

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
