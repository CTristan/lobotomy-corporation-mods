// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using UnityEngine;

#endregion

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
