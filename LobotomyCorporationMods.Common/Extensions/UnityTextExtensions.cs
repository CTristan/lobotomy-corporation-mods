// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class UnityTextExtensions
    {
        public static void PreventTextFromResizing([NotNull] this Text text)
        {
            Guard.Against.Null(text, nameof(text));

            var contentSizeFitter = text.gameObject.GetComponentInChildren<ContentSizeFitter>()
                ? text.gameObject.GetComponentInChildren<ContentSizeFitter>()
                : text.gameObject.AddComponent<ContentSizeFitter>();

            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
