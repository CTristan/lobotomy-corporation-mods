// SPDX-License-Identifier: MIT

using CommandWindow;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IManagementSlotTestAdapter : IComponentTestAdapter<ManagementSlot>
    {
        string Name { get; }
    }
}
