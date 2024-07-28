// SPDX-License-Identifier: MIT

using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    /// <summary>Wrapper class to handle setting up the initial plumbing for a new button.</summary>
    public class UiButtonWithText : Button
    {
        protected Text Text { get; private set; }
        public float Height => image.rectTransform.rect.height;
        public float Width => image.rectTransform.rect.width;

        public new void Awake()
        {
            base.Awake();

            image = gameObject.AddComponent<Image>();
            SetButtonImage();

            Text = gameObject.CreateNewTextObject();
        }

        public void SetTextColor(Color color)
        {
            Text.color = color;
        }

        protected void SetButtonImage([CanBeNull] string imagePath = null)
        {
            var texture2D = new Texture2D(2, 2);

            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
            }

            image.sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2());
            targetGraphic = image;
            image.SetSize(texture2D.width, texture2D.height);
        }
    }
}
