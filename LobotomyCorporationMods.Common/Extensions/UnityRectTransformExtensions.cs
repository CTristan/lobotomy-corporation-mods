// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class UnityRectTransformExtensions
    {
        internal static void CopyRectTransform([NotNull] this RectTransform rectTransform,
            [NotNull] RectTransform rectTransformToCopy)
        {
            Guard.Against.Null(rectTransform, nameof(rectTransform));
            Guard.Against.Null(rectTransformToCopy, nameof(rectTransformToCopy));

            rectTransform.CopyTransform(rectTransformToCopy);

            rectTransform.anchoredPosition = rectTransformToCopy.anchoredPosition;
            rectTransform.anchoredPosition3D = rectTransformToCopy.anchoredPosition3D;
            rectTransform.anchorMax = rectTransformToCopy.anchorMax;
            rectTransform.anchorMin = rectTransformToCopy.anchorMin;
            rectTransform.offsetMax = rectTransformToCopy.offsetMax;
            rectTransform.offsetMin = rectTransformToCopy.offsetMin;
            rectTransform.sizeDelta = rectTransformToCopy.sizeDelta;
        }
    }
}
