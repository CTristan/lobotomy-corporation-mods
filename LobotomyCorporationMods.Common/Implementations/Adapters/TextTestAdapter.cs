// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class TextTestAdapter : Adapter<Text>, ITextTestAdapter
    {
        internal TextTestAdapter([NotNull] Text text)
        {
            GameObject = text;
        }

        public string Text
        {
            get =>
                GameObject.text;
            set =>
                GameObject.text = value;
        }
    }
}
