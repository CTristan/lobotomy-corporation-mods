// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes.ValidCodeCoverageExceptionAttributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

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
            _gameObject.SetDynamicTooltip(str);
        }
    }
}
