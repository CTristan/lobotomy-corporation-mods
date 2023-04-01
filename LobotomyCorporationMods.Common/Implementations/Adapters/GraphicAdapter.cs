// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public class GraphicAdapter : ComponentAdapter, IGraphicAdapter
    {
        private Graphic? _graphic;

        public new Graphic GameObject
        {
            get
            {
                if (_graphic is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _graphic;
            }
            set => _graphic = value;
        }
    }
}
