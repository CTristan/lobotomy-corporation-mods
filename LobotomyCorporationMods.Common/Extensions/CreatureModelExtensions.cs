// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace Hemocode.Common.Extensions
{
    public static class CreatureModelExtensions
    {
        internal static CreatureEquipmentMakeInfo GetAbnormalityGift([NotNull] this CreatureModel creatureModel)
        {
            return creatureModel.metaInfo.GetGift();
        }
    }
}
