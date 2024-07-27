// SPDX-License-Identifier: MIT

using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityImageExtensions
    {
        public static void SetImage([NotNull] this Image image,
            [NotNull] string imagePath)
        {
            Guard.Against.Null(image, nameof(image));

            var texture2D = new Texture2D(2, 2);

            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
            }

            image.sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2());
            image.SetSize(texture2D.width, texture2D.height);
        }

        public static void SetPosition([NotNull] this Image image,
            float x,
            float y)
        {
            Guard.Against.Null(image, nameof(image));

            image.gameObject.transform.localPosition = new Vector2(x, y);
        }

        public static void SetSize([NotNull] this Image image,
            float width,
            float height)
        {
            Guard.Against.Null(image, nameof(image));

            image.rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}
