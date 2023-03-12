// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class TextAdapter : GraphicAdapter, ITextAdapter
    {
        private Text? _text;

        public new Text GameObject
        {
            get
            {
                if (_text is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _text;
            }
            set => _text = value;
        }

        public string Text
        {
            get => GameObject.text;
            set => GameObject.text = value;
        }
    }
}
