// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using CommandWindow;
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
    internal sealed class ManagementSlotTestAdapter : ComponentTestAdapter<ManagementSlot>, IManagementSlotTestAdapter
    {
        internal ManagementSlotTestAdapter([NotNull] ManagementSlot gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public string Name => _gameObject.name;
    }
}
