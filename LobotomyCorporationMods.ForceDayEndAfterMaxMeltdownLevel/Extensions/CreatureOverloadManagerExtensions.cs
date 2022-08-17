// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace LobotomyCorporationMods.ForceDayEndAfterMaxMeltdownLevel.Extensions
{
    internal static class CreatureOverloadManagerExtensions
    {
        internal static bool IsMaxMeltdown([NotNull] this CreatureOverloadManager creatureOverloadManager)
        {
            const int MaxMeltdownLevel = 10;
            var meltdownLevel = creatureOverloadManager.GetQliphothOverloadLevel();

            return meltdownLevel >= MaxMeltdownLevel;
        }
    }
}
