// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Extensions
{
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal static class ListExtensions
    {
        /// <summary>Returns a random element from <paramref name="list" />.</summary>
        /// <exception cref="System.ArgumentNullException">When <paramref name="list" /> is null.</exception>
        /// <exception cref="System.ArgumentException">When <paramref name="list" /> is empty.</exception>
        internal static T GetRandom<T>(this IList<T> list)
        {
            ThrowHelper.ThrowIfNull(list, nameof(list));
            if (list.Count == 0)
            {
                throw new System.ArgumentException("List is empty.", nameof(list));
            }

            return list[Random.Range(0, list.Count)];
        }
    }
}
