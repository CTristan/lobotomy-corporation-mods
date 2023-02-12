// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter
    {
        int UpgradeAgentStat(int originalStatLevel, int currentStatLevel, int statLevelIncrease);
    }
}
