// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowAdapter : IAdapter<CustomizingWindow>, IComponentAdapter
    {
        new CustomizingWindow GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        void OpenAppearanceWindow();
        int SetRandomStatValue(int original, int currentLevel, int bonusLevel);
    }
}
