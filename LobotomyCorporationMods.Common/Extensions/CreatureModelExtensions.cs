// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class CreatureModelExtensions
    {
        internal static CreatureEquipmentMakeInfo GetAbnormalityGift([NotNull] this CreatureModel creatureModel)
        {
            return creatureModel.metaInfo.GetGift();
        }
    }
}
