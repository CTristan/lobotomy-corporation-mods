// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions
{
    internal static class PlayerModelExtensions
    {
        internal static bool HasEnoughEnergy([NotNull] this PlayerModel playerModel, [NotNull] StageTypeInfo stageTypeInfo, [NotNull] EnergyModel energyModel)
        {
            Guard.Against.Null(playerModel, nameof(playerModel));
            Guard.Against.Null(stageTypeInfo, nameof(stageTypeInfo));
            Guard.Against.Null(energyModel, nameof(energyModel));

            var day = playerModel.GetDay();
            var energyNeed = stageTypeInfo.GetEnergyNeed(day);
            var energy = energyModel.GetEnergy();

            return energy >= energyNeed;
        }
    }
}
