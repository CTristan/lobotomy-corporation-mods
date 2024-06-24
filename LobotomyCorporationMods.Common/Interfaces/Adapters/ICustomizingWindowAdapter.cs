// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter : IAdapter<CustomizingWindow>
    {
        void OpenAppearanceWindow();
        int SetRandomStatValue(int original, int currentLevel, int bonusLevel);
    }
}
