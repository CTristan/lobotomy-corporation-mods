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
    public sealed class ImageTestAdapter : ComponentTestAdapter<Image>, IImageTestAdapter
    {
        internal ImageTestAdapter([NotNull] Image gameObject) : base(gameObject)
        {
        }

        public Color Color
        {
            get =>
                GameObjectInternal.color;
            set =>
                GameObjectInternal.color = value;
        }

        public Sprite Sprite
        {
            get =>
                GameObjectInternal.sprite;
            set =>
                GameObjectInternal.sprite = value;
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter AddTooltipMouseOverComponent()
        {
            return new TooltipMouseOverTestAdapter(GameObjectInternal.gameObject.AddComponent<TooltipMouseOver>());
        }

        [NotNull]
        public ITooltipMouseOverTestAdapter TooltipMouseOverComponent => new TooltipMouseOverTestAdapter(GameObjectInternal.gameObject.GetComponent<TooltipMouseOver>());
    }
}
