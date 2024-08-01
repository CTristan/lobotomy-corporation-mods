// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;

#endregion

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
