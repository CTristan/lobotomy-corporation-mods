// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions
{
    internal static class CreatureModelExtensions
    {
        internal static bool IsRoomInMeltdown([NotNull] this CreatureModel creature)
        {
            // If the abnormality is under meltdown, allow working so the player can take care of it
            var room = creature.Unit.room;

            return room.overloadUI.isActivated;
        }
    }
}
