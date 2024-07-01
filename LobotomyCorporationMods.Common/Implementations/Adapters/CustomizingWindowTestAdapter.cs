// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class CustomizingWindowTestAdapter : Adapter<CustomizingWindow>, ICustomizingWindowTestAdapter
    {
        public void OpenAppearanceWindow()
        {
            GameObject.OpenAppearanceWindow();
        }

        public int SetRandomStatValue(int original,
            int currentLevel,
            int bonusLevel)
        {
            return GameObject.SetRandomStatValue(original, currentLevel, bonusLevel);
        }
    }
}
