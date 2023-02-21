// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter
    {
        void OpenAppearanceWindow();
        int SetRandomStatValue(int original, int currentLevel, int bonusLevel);
    }
}
