// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class TextTestAdapter : Adapter<Text>, ITextTestAdapter
    {
        public string Text
        {
            get =>
                GameObject.text;
            set =>
                GameObject.text = value;
        }
    }
}