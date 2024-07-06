// SPDX-License-Identifier: MIT

using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    internal sealed class ManagementSlotTestAdapter : ComponentTestAdapter<ManagementSlot>, IManagementSlotTestAdapter
    {
        internal ManagementSlotTestAdapter([NotNull] ManagementSlot gameObject) : base(gameObject)
        {
        }

        public string Name => GameObject.name;
    }
}
