// SPDX-License-Identifier: MIT

#region

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter
    {
        void OpenAppearanceWindow();
        int SetRandomStatValue(int original, int currentLevel, int bonusLevel);
    }
}
