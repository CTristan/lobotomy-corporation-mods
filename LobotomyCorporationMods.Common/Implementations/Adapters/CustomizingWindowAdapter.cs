// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    public sealed class CustomizingWindowAdapter : ICustomizingWindowAdapter
    {
        private readonly CustomizingWindow _customizingWindow;

        public CustomizingWindowAdapter(CustomizingWindow customizingWindow)
        {
            _customizingWindow = customizingWindow;
        }

        public int UpgradeAgentStat(int originalStatLevel, int currentStatLevel, int statLevelIncrease)
        {
            return _customizingWindow.SetRandomStatValue(originalStatLevel, currentStatLevel, statLevelIncrease);
        }
    }
}
