// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class CustomizingWindowTestAdapter : ComponentTestAdapter<CustomizingWindow>, ICustomizingWindowTestAdapter
    {
        internal CustomizingWindowTestAdapter([NotNull] CustomizingWindow customizingWindow) : base(customizingWindow)
        {
        }

        public void OpenAppearanceWindow()
        {
            GameObjectInternal.OpenAppearanceWindow();
        }

        public int SetRandomStatValue(int original,
            int currentLevel,
            int bonusLevel)
        {
            return GameObjectInternal.SetRandomStatValue(original, currentLevel, bonusLevel);
        }
    }
}
