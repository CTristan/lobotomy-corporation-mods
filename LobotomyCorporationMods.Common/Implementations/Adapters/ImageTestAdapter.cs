// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class ImageTestAdapter : ComponentTestAdapter<Image>, IImageTestAdapter
    {
        internal ImageTestAdapter([NotNull] Image gameObject) : base(gameObject)
        {
        }

        public Color Color
        {
            get =>
                GameObject.color;
            set =>
                GameObject.color = value;
        }

        public Sprite Sprite
        {
            get =>
                GameObject.sprite;
            set =>
                GameObject.sprite = value;
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter AddTooltipMouseOverComponent()
        {
            return new TooltipMouseOverTestAdapter(GameObject.gameObject.AddComponent<TooltipMouseOver>());
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter TooltipMouseOverComponent => new TooltipMouseOverTestAdapter(GameObject.gameObject.GetComponent<TooltipMouseOver>());
    }
}
