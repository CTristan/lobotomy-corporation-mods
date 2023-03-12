// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class BeautyBeastAnimAdapter : ComponentAdapter, IBeautyBeastAnimAdapter
    {
        private BeautyBeastAnim? _beautyBeastAnim;

        public new BeautyBeastAnim GameObject
        {
            get
            {
                if (_beautyBeastAnim is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _beautyBeastAnim;
            }
            set => _beautyBeastAnim = value;
        }

        public int State => GameObject.GetState();
    }
}
