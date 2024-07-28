// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    public sealed class UiPresetList : MonoBehaviour
    {
        private const int NumberOfPresetsPerPage = 5;
        private string _arrowIconPath;
        private int _currentPage;
        private GameObject _downArrow;
        private List<PresetSlotButton> _panelButtonList;
        private List<KeyValuePair<string, PresetData>> _presets;
        private GameObject _upArrow;

        [UsedImplicitly]
        public void Awake()
        {
            try
            {
                InitializeArrows();
                ReloadPresets();
                InitializePanelButtonsList();
                UpdatePage();
            }
            catch (Exception exception)
            {
                Harmony_Patch.Instance.Logger.WriteException(exception);
                throw;
            }
        }

        private void ReloadPresets()
        {
            _presets = new List<KeyValuePair<string, PresetData>>(Harmony_Patch.Instance.PresetLoader.Presets.OrderBy(x => x.Key));
        }

        private void InitializeArrows()
        {
            _arrowIconPath = Application.dataPath + "/Managed/BaseMod/Image/Down.png";

            if (_downArrow.IsUnityNull())
            {
                _downArrow = MakeDownButton();
            }

            if (_upArrow.IsUnityNull())
            {
                _upArrow = MakeUpButton();
            }
        }

        private void InitializePanelButtonsList()
        {
            _panelButtonList = new List<PresetSlotButton>();
            for (var presetNum = 0; presetNum < NumberOfPresetsPerPage; presetNum++)
            {
                var hasPresetAtIndex = presetNum < _presets.Count;
                var presetName = hasPresetAtIndex ? _presets[presetNum].Key : string.Empty;
                var presetSlotButton = new GameObject().AddComponent<PresetSlotButton>();
                presetSlotButton.transform.SetParent(transform);
                presetSlotButton.UpdateButton(this, presetNum, presetName);
                _panelButtonList.Add(presetSlotButton);
            }
        }

        /// <summary>Updates the page of the preset list.</summary>
        internal void UpdatePage()
        {
            ReloadPresets();
            var pageStartIndex = _currentPage * NumberOfPresetsPerPage;

            _upArrow.SetActive(_currentPage != 0);

            for (var presetPanelNum = 0; presetPanelNum < NumberOfPresetsPerPage; presetPanelNum++)
            {
                _panelButtonList[presetPanelNum].onClick.RemoveAllListeners();

                var presetIndex = pageStartIndex + presetPanelNum;
                var hasPresetAtIndex = presetIndex < _presets.Count;
                if (hasPresetAtIndex)
                {
                    var presetName = _presets[presetIndex].Key;
                    _panelButtonList[presetPanelNum].UpdateButton(this, presetPanelNum, presetName);
                }
                else
                {
                    _panelButtonList[presetPanelNum].ClearButton();
                }
            }

            var hasMorePagesToShow = pageStartIndex + NumberOfPresetsPerPage < _presets.Count;
            _downArrow.SetActive(hasMorePagesToShow);
        }

        internal void OnClickDownButton()
        {
            if ((_currentPage + 1) * NumberOfPresetsPerPage >= _presets.Count)
            {
                return;
            }

            _currentPage++;
            UpdatePage();
        }

        [NotNull]
        private GameObject MakeDownButton()
        {
            const float YPosition = -300f;
            var downButtonGameObject = new GameObject("Down");
            var image = downButtonGameObject.AddComponent<Image>();
            downButtonGameObject.transform.SetParent(gameObject.transform);
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(_arrowIconPath));
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
            image.sprite = sprite;
            image.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
            downButtonGameObject.transform.localScale = new Vector3(1f, 1f);
            var button = downButtonGameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OnClickDownButton);
            downButtonGameObject.transform.localPosition = new Vector2(0.0f, YPosition);

            return downButtonGameObject;
        }

        internal void OnClickUpButton()
        {
            if (_currentPage == 0)
            {
                return;
            }

            _currentPage--;
            UpdatePage();
        }

        [NotNull]
        private GameObject MakeUpButton()
        {
            var upButtonGameObject = new GameObject("Down");
            var image = upButtonGameObject.AddComponent<Image>();
            upButtonGameObject.transform.SetParent(gameObject.transform);
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(_arrowIconPath));
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
            image.sprite = sprite;
            image.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
            upButtonGameObject.transform.localScale = new Vector3(1f, -1f);
            var button = upButtonGameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OnClickUpButton);
            upButtonGameObject.transform.localPosition = new Vector2(0.0f, 250f);

            return upButtonGameObject;
        }
    }
}
