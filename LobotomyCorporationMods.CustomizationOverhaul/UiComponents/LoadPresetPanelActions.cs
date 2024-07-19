// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal static class LoadPresetPanelActions
    {
        internal static void TogglePanelVisibility()
        {
            if (Harmony_Patch.Instance.LoadPresetPanel.IsNull())
            {
                var gameObject = new GameObject("BackGround");
                var image = gameObject.AddComponent<Image>();
                image.transform.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                var texture2D = new Texture2D(2, 2);
                var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
                image.sprite = sprite;
                image.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
                gameObject.transform.localScale = new Vector3(1f, 1f);
                gameObject.transform.localPosition = new Vector3(PresetConstants.LoadPresetPanelPositionX, PresetConstants.LoadPresetPanelPositionY);
                gameObject.AddComponent<UiPresetList>();
                gameObject.SetActive(true);
                Harmony_Patch.Instance.LoadPresetPanel = gameObject;

                var list = new UiPresetList();

                return;
            }

            Harmony_Patch.Instance.LoadPresetPanel.SetActive(!Harmony_Patch.Instance.LoadPresetPanel.activeSelf);
        }
    }
}
