// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class TooltipMouseOverTestAdapter : ComponentTestAdapter<TooltipMouseOver>, ITooltipMouseOverTestAdapter
    {
        internal TooltipMouseOverTestAdapter([NotNull] TooltipMouseOver gameObject) : base(gameObject)
        {
        }

        public void SetDynamicTooltip(string str)
        {
            GameObject.SetDynamicTooltip(str);
        }
    }
}
