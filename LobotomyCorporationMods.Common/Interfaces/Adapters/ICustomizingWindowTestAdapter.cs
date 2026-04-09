// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowTestAdapter : IComponentTestAdapter<CustomizingWindow>
    {
        void OpenAppearanceWindow();

        int SetRandomStatValue(int original, int currentLevel, int bonusLevel);
    }
}
