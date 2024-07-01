// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class YggdrasilAnimTestAdapter : Adapter<YggdrasilAnim>, IYggdrasilAnimTestAdapter
    {
        [NotNull]
        public IEnumerable<IGameObjectTestAdapter> Flowers
        {
            get
            {
                var flowers = GameObject.flowers;

                return flowers.Select(flower => new GameObjectTestAdapter
                {
                    GameObject = flower,
                }).Cast<IGameObjectTestAdapter>();
            }
        }
    }
}
