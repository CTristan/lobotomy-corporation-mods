// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class ManagementSlotTestAdapter : ComponentTestAdapter<ManagementSlot>, IManagementSlotTestAdapter
    {
        internal ManagementSlotTestAdapter([NotNull] ManagementSlot gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public string Name =>
            GameObjectInternal.name;
    }
}
