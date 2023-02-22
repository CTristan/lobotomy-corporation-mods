// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    [AdapterClass]
    public sealed class CustomizingWindowAdapter : Adapter<CustomizingWindow>, ICustomizingWindowAdapter
    {
        public void OpenAppearanceWindow()
        {
            GameObject.OpenAppearanceWindow();
        }

        public int SetRandomStatValue(int original, int currentLevel, int bonusLevel)
        {
            return GameObject.SetRandomStatValue(original, currentLevel, bonusLevel);
        }
    }
}
