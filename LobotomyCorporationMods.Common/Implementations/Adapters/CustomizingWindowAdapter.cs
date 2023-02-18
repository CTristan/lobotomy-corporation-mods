// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class CustomizingWindowAdapter : ICustomizingWindowAdapter
    {
        private readonly CustomizingWindow _customizingWindow;

        public CustomizingWindowAdapter(CustomizingWindow customizingWindow)
        {
            _customizingWindow = customizingWindow;
        }

        public void OpenAppearanceWindow()
        {
            _customizingWindow.OpenAppearanceWindow();
        }

        public int SetRandomStatValue(int original, int currentLevel, int bonusLevel)
        {
            return _customizingWindow.SetRandomStatValue(original, currentLevel, bonusLevel);
        }
    }
}
