// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Facades;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public sealed class CreatureEquipmentMakeInfoFacade : ICreatureEquipmentMakeInfoFacade
    {
        private readonly CreatureEquipmentMakeInfo _creatureEquipmentMakeInfo;

        public CreatureEquipmentMakeInfoFacade(CreatureEquipmentMakeInfo creatureEquipmentMakeInfo)
        {
            _creatureEquipmentMakeInfo = creatureEquipmentMakeInfo;
        }

        [NotNull]
        public string GiftName => _creatureEquipmentMakeInfo.equipTypeInfo?.Name ?? string.Empty;
    }
}
