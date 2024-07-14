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
                _gameObject.color;
            set =>
                _gameObject.color = value;
        }

        public Sprite Sprite
        {
            get =>
                _gameObject.sprite;
            set =>
                _gameObject.sprite = value;
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter AddTooltipMouseOverComponent()
        {
            return new TooltipMouseOverTestAdapter(_gameObject.gameObject.AddComponent<TooltipMouseOver>());
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter TooltipMouseOverComponent => new TooltipMouseOverTestAdapter(_gameObject.gameObject.GetComponent<TooltipMouseOver>());
    }
}
