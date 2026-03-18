// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class TooltipMouseOverTestAdapter : ComponentTestAdapter<TooltipMouseOver>, ITooltipMouseOverTestAdapter
    {
        internal TooltipMouseOverTestAdapter([NotNull] TooltipMouseOver gameObject) : base(gameObject)
        {
        }

        public void SetDynamicTooltip(string str)
        {
            GameObjectInternal.SetDynamicTooltip(str);
        }
    }
}
