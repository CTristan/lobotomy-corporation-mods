// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class TextAdapter : ITextAdapter
    {
        private readonly Text _text;

        public TextAdapter(Text text)
        {
            _text = text;
        }

        public string Text
        {
            get => _text.text;
            set => _text.text = value;
        }
    }
}
