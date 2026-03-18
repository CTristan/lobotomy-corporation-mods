// SPDX-License-Identifier: MIT

#region

using Customizing;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface ICustomizingWindowTestAdapter : IComponentTestAdapter<CustomizingWindow>
    {
        void OpenAppearanceWindow();

        int SetRandomStatValue(int original,
            int currentLevel,
            int bonusLevel);
    }
}
