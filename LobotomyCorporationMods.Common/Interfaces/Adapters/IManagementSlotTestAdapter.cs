// SPDX-License-Identifier: MIT

#region

using CommandWindow;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IManagementSlotTestAdapter : IComponentTestAdapter<ManagementSlot>
    {
        string Name { get; }
    }
}
