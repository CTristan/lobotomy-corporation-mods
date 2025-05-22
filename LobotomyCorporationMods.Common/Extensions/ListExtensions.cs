// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class ListExtensions
    {
        public static T GetRandom<T>([NotNull] this List<T> list)
        {
            Guard.Against.Null(list, nameof(list));

            return list[Random.Range(0, list.Count)];
        }
    }
}
