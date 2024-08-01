// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Implementations.UiComponents
{
    /// <summary>Wrapper class to handle setting up the initial plumbing for a new button.</summary>
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class UiButton : Button
    {
        public float Height => image.rectTransform.rect.height;
        public float Width => image.rectTransform.rect.width;

        public new void Awake()
        {
            base.Awake();
            image = gameObject.AddComponent<Image>();
            SetButtonImage();
        }

        public void SetButtonImage([CanBeNull] string imagePath = null)
        {
            var texture2D = new Texture2D(2, 2);
            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
            }

            image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
            targetGraphic = image;
            image.SetSize(texture2D.width, texture2D.height);
        }

        public void SetPosition(float x,
            float y)
        {
            image.SetPosition(x, y);
        }
    }
}
