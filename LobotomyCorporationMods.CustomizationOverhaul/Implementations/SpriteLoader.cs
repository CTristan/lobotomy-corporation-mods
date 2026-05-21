// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using UnityEngine;

namespace LobotomyCorporationMods.CustomizationOverhaul.Implementations
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal static class SpriteLoader
    {
        [NotNull]
        internal static Sprite LoadSpriteFromFile([NotNull] string path)
        {
            ThrowHelper.ThrowIfNull(path, nameof(path));

            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}
