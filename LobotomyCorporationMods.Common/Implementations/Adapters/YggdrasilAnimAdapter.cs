// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class YggdrasilAnimAdapter : IYggdrasilAnimAdapter
    {
        public YggdrasilAnimAdapter(YggdrasilAnim? animationScript)
        {
            if (animationScript is not null)
            {
                foreach (var flower in animationScript.flowers)
                {
                    Flowers.Add(new GameObjectAdapter(flower));
                }
            }
        }

        public IEnumerable<IGameObjectAdapter> Flowers { get; } = new List<IGameObjectAdapter>();
    }
}
