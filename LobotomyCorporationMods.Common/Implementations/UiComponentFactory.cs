// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.UiComponents;
using LobotomyCorporationMods.Common.Interfaces.UiComponents;

namespace LobotomyCorporationMods.Common.Implementations
{
    public static class UiComponentFactory
    {
        [NotNull]
        public static IUiButton CreateUiButton()
        {
            return new UiButton();
        }

        [NotNull]
        internal static IUiText CreateUiText()
        {
            return new UiText();
        }
    }
}
