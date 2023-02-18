// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Extensions;

#endregion

namespace LobotomyCorporationMods.BugFixes.Extensions
{
    internal static class UnitModelExtensions
    {
        internal static bool HasCrumblingArmor([NotNull] this UnitModel unit)
        {
            return unit.HasEquipment(EquipmentId.CrumblingArmorGift1) || unit.HasEquipment(EquipmentId.CrumblingArmorGift2) || unit.HasEquipment(EquipmentId.CrumblingArmorGift3) ||
                   unit.HasEquipment(EquipmentId.CrumblingArmorGift4);
        }
    }
}
