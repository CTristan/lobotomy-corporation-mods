// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal sealed class UiPresetList : MonoBehaviour
    {
        private const int NumberOfPresetsPerPage = 5;
        private readonly string _arrowIconPath = Application.dataPath + "/Managed/BaseMod/Image/Down.png";
        private int _currentPage;
        private IUiButton _downArrow;
        private List<IUiButton> _panelButtonList;
        private List<KeyValuePair<string, PresetData>> _presets;
        private GameObject _upArrow;

        [UsedImplicitly]
        public void Awake()
        {
            UpdatePage();
        }

        private void ReloadPresets()
        {
            _presets = new List<KeyValuePair<string, PresetData>>(Harmony_Patch.Instance.PresetLoader.Presets.OrderBy(x => x.Key));
        }

        private void ReinitializeComponents()
        {
            ReloadPresets();
            InitializeArrows();
            InitializePanelButtonsList();
        }

        private void InitializeArrows()
        {
            if (_downArrow.IsNull())
            {
                _downArrow = MakeDownButton();
            }

            if (_upArrow.IsNull())
            {
                _upArrow = MakeUpButton();
            }
        }

        private void InitializePanelButtonsList()
        {
            if (_panelButtonList.IsNull())
            {
                return;
            }

            _panelButtonList = new List<IUiButton>();
            for (var presetNum = 0; presetNum < NumberOfPresetsPerPage; presetNum++)
            {
                var hasPresetAtIndex = presetNum < _presets.Count;
                var presetName = hasPresetAtIndex ? _presets[presetNum].Key : string.Empty;
                _panelButtonList.Add(CreatePresetButton(presetNum, presetName));
            }
        }

        [NotNull]
        public IUiButton CreatePresetButton(int buttonNum,
            string presetName)
        {
            var button = UiComponentFactory.CreateUiButton();
            button.SetParent(gameObject.transform);
            button.Text = presetName;
            button.TextColor = PresetConstants.PresetTextColor;
            button.TextFontSize = PresetConstants.ButtonTextFontSize;
            button.TextFont = DeployUI.instance.ordeal.font;
            button.TextAlignment = TextAnchor.MiddleCenter;

            var fileManager = Harmony_Patch.Instance.FileManager;
            var imagePath = fileManager.GetFile("Assets/preset-panel.png");
            button.SetButtonImage(imagePath);
            button.SetPosition(0.0f, PresetConstants.LoadPresetPanelPositionY - buttonNum * button.Height);

            button.OnClick.AddListener(delegate
            {
                var loadedAgentData = Harmony_Patch.Instance.PresetLoader.LoadPreset(presetName);

                var instance = CustomizingWindow.CurrentWindow.appearanceUI;
                instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
                instance.SetAppearanceSprite(loadedAgentData);
                instance.SetCreditControl(true);

                Harmony_Patch.Instance.PresetSaver.UpdateSavePresetButtonText(presetName, loadedAgentData.appearance);
            });

            return button;
        }

        /// <summary>Updates the page of the preset list.</summary>
        public void UpdatePage()
        {
            ReinitializeComponents();
            var pageStartIndex = _currentPage * NumberOfPresetsPerPage;

            _upArrow.SetActive(_currentPage != 0);

            for (var presetPanelNum = 0; presetPanelNum < NumberOfPresetsPerPage; presetPanelNum++)
            {
                _panelButtonList[presetPanelNum].OnClick.RemoveAllListeners();

                var presetIndex = pageStartIndex + presetPanelNum;
                var hasPresetAtIndex = presetIndex < _presets.Count;
                if (hasPresetAtIndex)
                {
                    var presetName = _presets[presetIndex].Key;
                    _panelButtonList[presetPanelNum].Text = _presets[presetIndex].Key;
                    _panelButtonList[presetPanelNum].OnClick.AddListener(delegate
                    {
                        var loadedAgentData = Harmony_Patch.Instance.PresetLoader.LoadPreset(presetName);

                        var instance = CustomizingWindow.CurrentWindow.appearanceUI;
                        instance.palette.OnSetColor(loadedAgentData.appearance.HairColor);
                        instance.SetAppearanceSprite(loadedAgentData);
                        instance.SetCreditControl(true);

                        Harmony_Patch.Instance.PresetSaver.UpdateSavePresetButtonText(presetName, loadedAgentData.appearance);
                    });
                }
                else
                {
                    _panelButtonList[presetPanelNum].Text = string.Empty;
                }
            }

            var hasMorePagesToShow = pageStartIndex + NumberOfPresetsPerPage < _presets.Count;
            _downArrow.SetActive(hasMorePagesToShow);
        }

        public void OnClickDownButton()
        {
            if ((_currentPage + 1) * NumberOfPresetsPerPage >= _presets.Count)
            {
                return;
            }

            _currentPage++;
            UpdatePage();
        }

        [NotNull]
        public GameObject MakeDownButton()
        {
            var downButtonGameObject = new GameObject("Down");
            var image = downButtonGameObject.AddComponent<Image>();
            downButtonGameObject.transform.SetParent(gameObject.transform);
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(Application.dataPath + "/Managed/BaseMod/Image/Down.png"));
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
            image.sprite = sprite;
            image.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
            downButtonGameObject.transform.localScale = new Vector3(1f, 1f);
            var button = downButtonGameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OnClickDownButton);
            downButtonGameObject.transform.localPosition = new Vector2(0.0f, -300f);

            return downButtonGameObject;
        }

        public void OnClickUpButton()
        {
            if (_currentPage == 0)
            {
                return;
            }

            _currentPage--;
            UpdatePage();
        }

        [NotNull]
        public GameObject MakeUpButton()
        {
            var upButtonGameObject = new GameObject("Down");
            var image = upButtonGameObject.AddComponent<Image>();
            upButtonGameObject.transform.SetParent(gameObject.transform);
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(Application.dataPath + "/Managed/BaseMod/Image/Down.png"));
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
