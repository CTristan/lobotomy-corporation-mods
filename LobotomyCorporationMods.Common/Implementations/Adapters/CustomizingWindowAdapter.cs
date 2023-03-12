// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class CustomizingWindowAdapter : ComponentAdapter, ICustomizingWindowAdapter
    {
        private CustomizingWindow? _customizingWindow;

        public new CustomizingWindow GameObject
        {
            get
            {
                if (_customizingWindow is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _customizingWindow;
            }
            set => _customizingWindow = value;
        }

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
