// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using LobotomyCorporationMods.Common.Extensions;
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
            try
            {
                return _customizingWindow.SetRandomStatValue(originalStatLevel, currentStatLevel, statLevelIncrease);
            }
            catch (Exception ex) when (ex.IsUnityException())
            {
                return 0;
            }
        }
    }
}
