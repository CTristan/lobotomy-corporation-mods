// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class YggdrasilAnimAdapter : Adapter<YggdrasilAnim>, IYggdrasilAnimAdapter
    {
        [NotNull]
        public IEnumerable<IGameObjectAdapter> Flowers
        {
            get
            {
                var flowers = GameObject.flowers;

                return flowers.Select(flower => new GameObjectAdapter
                {
                    GameObject = flower,
                }).Cast<IGameObjectAdapter>();
            }
        }
    }
}
