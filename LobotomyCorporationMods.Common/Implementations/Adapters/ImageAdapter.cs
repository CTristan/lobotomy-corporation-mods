// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class ImageAdapter : IImageAdapter
    {
        private readonly Image _image;

        public ImageAdapter(Image image)
        {
            _image = image;
        }

        public Color Color
        {
            get => _image.color;
            set => _image.color = value;
        }
    }
}
