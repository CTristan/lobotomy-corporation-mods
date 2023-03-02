// SPDX-License-Identifier: MIT

#region

using System.Linq;
using LobotomyCorporationMods.Common.Extensions;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class CommandWindowExtensions
    {
        internal static CreatureEquipmentMakeInfo? GetCreatureGiftIfExists(this CommandWindow.CommandWindow commandWindow)
        {
            CreatureEquipmentMakeInfo? gift = null;

            if (commandWindow.TryGetCreature(out var creature) && creature is not null)
            {
                var equipment = creature.metaInfo.equipMakeInfos;
                gift = equipment.FirstOrDefault(static info => info.equipTypeInfo.type == EquipmentTypeInfo.EquipmentType.SPECIAL);
            }

            return gift;
        }
    }
}
