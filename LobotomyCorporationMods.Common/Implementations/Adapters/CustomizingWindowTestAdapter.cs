// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

#endregion

namespace Hemocode.Common.Implementations.Adapters
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
