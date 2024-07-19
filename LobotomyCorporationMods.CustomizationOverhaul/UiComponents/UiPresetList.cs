// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal sealed class UiPresetList : MonoBehaviour
    {
        public GameObject _downArrow;

        public int _page;

        public List<GameObject> _panelList;

        public List<Text> _panelTextList;
        public List<KeyValuePair<string, PresetData>> _presets;

        public GameObject _upArrow;

        [UsedImplicitly]
        public void Awake()
        {
            _presets = new List<KeyValuePair<string, PresetData>>(Harmony_Patch.Instance.PresetLoader.Presets);
            _page = 0;
            _panelList = new List<GameObject>();
            _panelTextList = new List<Text>();
            for (var i = 0; i < 5; i++)
            {
                var hasMorePresets = _presets.Count > i;
                var newGameObject = MakeModInfo(i);
                MakeModInfo2(hasMorePresets ? _presets[i].Key : null, newGameObject);

                _panelList.Add(newGameObject);
                newGameObject.transform.localPosition = new Vector2(-800f, 255 - i * 100);
            }

            _downArrow = MakeDownButton();
            _downArrow.transform.localPosition = new Vector2(-795f, -445f);
            _upArrow = MakeUpButton();
            _upArrow.transform.localPosition = new Vector2(-795f, 355f);
            UpdatePage();
        }

        [UsedImplicitly]
        public void Update()
        {
            if (gameObject.activeSelf)
            {
                return;
            }

            var x = _upArrow.transform.localPosition.x;
            var y = _upArrow.transform.localPosition.y;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                _upArrow.transform.localPosition = new Vector2(x, y + 1f);
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                _upArrow.transform.localPosition = new Vector2(x, y - 1f);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _upArrow.transform.localPosition = new Vector2(x - 1f, y);
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                _upArrow.transform.localPosition = new Vector2(x + 1f, y);
            }
        }

        [NotNull]
        public GameObject MakeModInfo(int i)
        {
            var gameObject = new GameObject("BackGround1");
            var image = gameObject.AddComponent<Image>();
            gameObject.transform.SetParent(this.gameObject.transform);
            var texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(File.ReadAllBytes(Application.dataPath + "/Managed/BaseMod/Image/Mod.png"));
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
            image.sprite = sprite;
            image.rectTransform.sizeDelta = new Vector2(texture2D.width, texture2D.height);
            gameObject.transform.localScale = new Vector3(1f, 1f);
            var button = gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            // button.onClick.AddListener(delegate
            // {
            //     OnClickModInfo(i);
            // });

            return gameObject;
        }

        [NotNull]
        public GameObject MakeModInfo2([CanBeNull] string presetName,
            [NotNull] GameObject Button)
        {
            var gameObject = new GameObject("InfoText");
            var text = gameObject.AddComponent<Text>();
            gameObject.transform.SetParent(Button.transform);
            text.rectTransform.sizeDelta = Vector2.zero;
            text.rectTransform.anchorMin = new Vector2(0.02f, 0f);
            text.rectTransform.anchorMax = new Vector2(0.98f, 1f);
            text.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            if (presetName != null)
            {
                text.text = presetName;
            }

            text.font = OptionUI.Instance.CreditTitle.font;
            text.fontSize = 30;
            text.color = new Color(0.2509804f, 1f, 0.654902f);
            text.alignment = TextAnchor.MiddleCenter;
            gameObject.transform.localScale = new Vector3(1f, 1f);
            gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject.SetActive(true);
            _panelTextList.Add(text);
            return gameObject;
        }

        public void UpdatePage()
        {
            if (_page == 0)
            {
                _upArrow.SetActive(false);
            }
            else
            {
                _upArrow.SetActive(true);
            }

            for (var i = 0; i < 5; i++)
            {
                if (_page * 5 + i >= _presets.Count)
                {
                    _panelTextList[i].text = "";
                }
                else
                {
                    _panelTextList[i].text = _presets[_page * 5 + i].Key;
                }
            }

            if (_page * 5 + 5 >= _presets.Count)
            {
                _downArrow.SetActive(false);
                return;
            }

            _downArrow.SetActive(true);
        }

        public void OnClickDownButton()
        {
            if ((_page + 1) * 5 >= _presets.Count)
            {
                return;
            }

            _page++;
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
            return downButtonGameObject;
        }

        public void OnClickUpButton()
        {
            if (_page == 0)
            {
                return;
            }

            _page--;
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
            return upButtonGameObject;
        }
    }
}
