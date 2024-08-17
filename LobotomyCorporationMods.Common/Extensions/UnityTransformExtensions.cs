// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class UnityTransformExtensions
    {
        internal static void CopyTransform([NotNull] this Transform transform,
            [NotNull] Transform transformToCopy)
        {
            Guard.Against.Null(transform, nameof(transform));
            Guard.Against.Null(transformToCopy, nameof(transformToCopy));

            transform.SetParent(transformToCopy.parent);

            transform.localScale = transformToCopy.localScale;
            transform.position = transformToCopy.position;
        }
    }
}
