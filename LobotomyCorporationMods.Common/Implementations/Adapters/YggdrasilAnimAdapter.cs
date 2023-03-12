// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class YggdrasilAnimAdapter : ComponentAdapter, IYggdrasilAnimAdapter
    {
        private YggdrasilAnim? _yggdrasilAnim;

        public IEnumerable<IGameObjectAdapter> Flowers
        {
            get
            {
                var flowers = GameObject.flowers;

                return flowers.Select(static flower => new GameObjectAdapter { GameObject = flower }).Cast<IGameObjectAdapter>();
            }
        }

        public new YggdrasilAnim GameObject
        {
            get
            {
                if (_yggdrasilAnim is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _yggdrasilAnim;
            }
            set => _yggdrasilAnim = value;
        }
    }
}
