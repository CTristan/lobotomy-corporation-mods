// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;
using UnityEngine.UI;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityGameObjectExtensions
    {
        [NotNull]
        public static Text CreateNewTextObject([NotNull] this GameObject gameObject)
        {
            Guard.Against.Null(gameObject, nameof(gameObject));

            var text = new GameObject().AddComponent<Text>();
            text.transform.SetParent(gameObject.transform);
            text.rectTransform.sizeDelta = new Vector2(0f, 0f);
            text.rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            text.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
            text.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);

            return text;
        }
    }
}
