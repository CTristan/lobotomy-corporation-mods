// SPDX-License-Identifier: MIT

#region

using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityImageExtensions
    {
        public static void CopyImage([NotNull] this Image image,
            [NotNull] Image imageToCopy,
            bool copySprite = false)
        {
            Guard.Against.Null(image, nameof(image));
            Guard.Against.Null(imageToCopy, nameof(imageToCopy));

            image.rectTransform.CopyRectTransform(imageToCopy.rectTransform);

            if (copySprite)
            {
                image.sprite = imageToCopy.sprite;
            }

            image.color = imageToCopy.color;
            image.fillMethod = imageToCopy.fillMethod;
            image.raycastTarget = imageToCopy.raycastTarget;
            image.type = imageToCopy.type;
        }

        public static float GetWidth([NotNull] this Image image)
        {
            Guard.Against.Null(image, nameof(image));

            return image.rectTransform.rect.width;
        }

        public static float GetHeight([NotNull] this Image image)
        {
            Guard.Against.Null(image, nameof(image));

            return image.rectTransform.rect.height;
        }

        public static void SetImage([NotNull] this Image image,
            [NotNull] string imagePath)
        {
            Guard.Against.Null(image, nameof(image));

            var texture2D = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            texture2D.filterMode = FilterMode.Trilinear;
            texture2D.Compress(true);

            if (!string.IsNullOrEmpty(imagePath))
            {
                texture2D.LoadImage(File.ReadAllBytes(imagePath));
                texture2D.Apply();
            }

            image.sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            image.SetSize(texture2D.width, texture2D.height);
        }

        public static void SetLocalPosition([NotNull] this Image image,
            float x,
            float y)
        {
            Guard.Against.Null(image, nameof(image));

            image.rectTransform.localPosition = new Vector2(x, y);
        }

        public static void SetScale([NotNull] this Image image,
            float scale)
        {
            Guard.Against.Null(image, nameof(image));

            image.rectTransform.localScale = new Vector3(scale, scale, scale);
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
