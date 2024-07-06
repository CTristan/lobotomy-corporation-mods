// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using CommandWindow;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Extensions
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    internal static class ManagementSlotExtensions
    {
        internal static CreatureEquipmentMakeInfo GetAbnormalityGift(this ManagementSlot managementSlot)
        {
            var commandWindow = CommandWindow.CommandWindow.CurrentWindow;

            return commandWindow.GetAbnormalityGift();
        }

        internal static EquipmentTypeInfo GetAbnormalityGiftInfo(this ManagementSlot managementSlot)
        {
            return managementSlot.GetAbnormalityGift().equipTypeInfo;
        }
    }
}
