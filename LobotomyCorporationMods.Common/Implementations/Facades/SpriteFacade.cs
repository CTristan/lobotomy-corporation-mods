// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class SpriteFacade
    {
        [NotNull]
        public static string GetSpriteName([NotNull] this Sprite sprite)
        {
            return sprite.GetName();
        }
    }
}
