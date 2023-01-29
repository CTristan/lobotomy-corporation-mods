// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    public static class CustomizingWindowExtensions
    {
        public static void SaveAgentAppearance([NotNull] this CustomizingWindow customizingWindow)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));

            if (customizingWindow.appearanceUI.copied != null)
            {
                customizingWindow.CurrentData.AppearCopy(customizingWindow.appearanceUI.copied);
                customizingWindow.appearanceUI.copied = null;
            }

            customizingWindow.CurrentAgent.SetAppearanceData(customizingWindow.CurrentData.appearance);
        }
    }
}
