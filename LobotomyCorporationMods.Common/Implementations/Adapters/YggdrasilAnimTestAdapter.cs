// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class YggdrasilAnimTestAdapter : ComponentTestAdapter<YggdrasilAnim>, IYggdrasilAnimTestAdapter
    {
        internal YggdrasilAnimTestAdapter([NotNull] YggdrasilAnim gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public IEnumerable<IGameObjectTestAdapter> Flowers
        {
            get
            {
                GameObject[] flowers = _gameObject.flowers;

                return flowers.Select(flower => new GameObjectTestAdapter(flower)).Cast<IGameObjectTestAdapter>();
            }
        }
    }
}
