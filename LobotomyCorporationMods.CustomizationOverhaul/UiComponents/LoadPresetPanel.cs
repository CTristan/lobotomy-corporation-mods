// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using LobotomyCorporationMods.CustomizationOverhaul.Constants;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.UiComponents
{
    internal sealed class LoadPresetPanel : IUiImage
    {
        internal LoadPresetPanel()
        {
            InitializeComponents();
        }

        private UiPresetList UiPresetList { get; set; }

        private IUiImage Image { get; set; }

        public bool IsActive => Image.IsActive;

        public T AddComponent<T>() where T : Component
        {
            return Image.AddComponent<T>();
        }

        public bool AnyComponentIsNull()
        {
            return Image.AnyComponentIsNull();
        }

        public void SetActive(bool value)
        {
            if (value)
            {
                UiPresetList.UpdatePage();
            }

            Image.SetActive(value);
        }

        public void SetParent(Transform parent)
        {
            Image.SetParent(parent);
        }

        public void SetPosition(float x,
            float y)
        {
            Image.SetPosition(x, y);
        }

        public void SetImageFromFile(string imagePath)
        {
            Image.SetImageFromFile(imagePath);
        }

        public void SetSize(float width,
            float height)
        {
            Image.SetSize(width, height);
        }

        public bool IsNotNull()
        {
            return Image.IsNotNull();
        }

        private void InitializeComponents()
        {
            if (Image.IsUnityNull())
            {
                Image = UiComponentFactory.CreateUiImage();
                Image.SetActive(true);
                Image.SetParent(AgentInfoWindow.currentWindow.gameObject.transform.GetChild(0));
                Image.SetPosition(PresetConstants.LoadPresetPanelPositionX, PresetConstants.LoadPresetPanelPositionY);
                Image.SetImageFromFile(Application.dataPath + "/Managed/BaseMod/Image/Back.png");
                Image.SetSize(PresetConstants.LoadPresetPanelSizeX, PresetConstants.LoadPresetPanelSizeY);
            }

            if (!UiPresetList.IsUnityNull())
            {
                return;
            }

            UiPresetList = new UiPresetList();
            UiPresetList = Image.AddComponent<UiPresetList>();
        }
    }
}
