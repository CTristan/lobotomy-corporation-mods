// SPDX-License-Identifier: MIT

using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    internal sealed class UiImage : UiComponent, IUiImage
    {
        internal UiImage()
        {
            ImageObject = GameObject.AddComponent<Image>();
        }

        private Image ImageObject { get; }

        public void SetImageFromFile([CanBeNull] string imagePath)
        {
            var texture2D = new Texture2D(2, 2);

            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
            }

            var sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2());
            ImageObject.sprite = sprite;
            SetSize(texture2D.width, texture2D.height);
        }

        public void SetSize(float width,
            float height)
        {
            ImageObject.rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
