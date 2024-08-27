// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.UiComponents
{
    [UiComponent]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public class ButtonWithText : UiButton
    {
        protected Image Image { get; private set; }
        protected Text Text { get; private set; }

        public new void Awake()
        {
            base.Awake();

            Image = new GameObject("Image").AddComponent<Image>();
            Image.transform.SetParent(transform);
            Text = new GameObject("Text").AddComponent<Text>();
            Text.transform.SetParent(Image.transform);

            Image.enabled = false;
        }


        public void CopyText([NotNull] Text textToCopy)
        {
            Guard.Against.Null(textToCopy, nameof(textToCopy));

            Text.alignment = textToCopy.alignment;
            Text.color = textToCopy.color;
            Text.font = textToCopy.font;
            Text.fontSize = textToCopy.fontSize;
            Text.fontStyle = textToCopy.fontStyle;
            Text.raycastTarget = textToCopy.raycastTarget;
            Text.resizeTextForBestFit = textToCopy.resizeTextForBestFit;
            Text.resizeTextMaxSize = textToCopy.resizeTextMaxSize;
            Text.resizeTextMinSize = textToCopy.resizeTextMinSize;
            Text.text = textToCopy.text;

            Text.rectTransform.offsetMax = textToCopy.rectTransform.offsetMax;
            Text.rectTransform.offsetMin = textToCopy.rectTransform.offsetMin;
            Text.rectTransform.sizeDelta = textToCopy.rectTransform.sizeDelta;

            Text.transform.localScale = textToCopy.transform.localScale;
        }

        public void SetText(string text)
        {
            Text.text = text;
        }

        public void SetTextAlignment(TextAnchor textAnchor)
        {
            Text.alignment = textAnchor;
        }

        public void SetTextColor(Color color)
        {
            Text.color = color;
        }

        public void SetTextFont(Font font)
        {
            Text.font = font;
        }

        public void SetTextFontSize(int fontSize)
        {
            Text.fontSize = fontSize;
        }
    }
}
