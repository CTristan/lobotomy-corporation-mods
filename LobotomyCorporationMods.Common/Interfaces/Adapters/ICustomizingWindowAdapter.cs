// SPDX-License-Identifier: MIT

#region

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter
    {
        int UpgradeAgentStat(int originalStatLevel, int currentStatLevel, int statLevelIncrease);
    }
}
