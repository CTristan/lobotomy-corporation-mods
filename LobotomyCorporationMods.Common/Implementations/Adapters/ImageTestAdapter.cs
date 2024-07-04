// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class ImageTestAdapter : Adapter<Image>, IImageTestAdapter
    {
        internal ImageTestAdapter([NotNull] Image image)
        {
            GameObject = image;
        }

        public Color Color
        {
            get =>
                GameObject.color;
            set =>
                GameObject.color = value;
        }
    }
}
