// SPDX-License-Identifier: MIT

using CommandWindow;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IManagementSlotTestAdapter : IComponentTestAdapter<ManagementSlot>
    {
        string Name { get; }
    }
}
