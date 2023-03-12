// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class ImageAdapter : GraphicAdapter, IImageAdapter
    {
        private Image? _image;

        public Color Color
        {
            get => GameObject.color;
            set => GameObject.color = value;
        }

        public new Image GameObject
        {
            get
            {
                if (_image is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _image;
            }
            set => _image = value;
        }

        public Sprite Sprite
        {
            get => GameObject.sprite;
            set => GameObject.sprite = value;
        }
    }
}
