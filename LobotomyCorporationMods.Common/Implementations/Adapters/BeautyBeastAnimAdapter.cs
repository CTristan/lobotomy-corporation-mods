// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class BeautyBeastAnimAdapter : IBeautyBeastAnimAdapter
    {
        private readonly BeautyBeastAnim? _animationScript;

        public BeautyBeastAnimAdapter(BeautyBeastAnim? animationScript)
        {
            _animationScript = animationScript;
        }

        private BeautyBeastAnim AnimationScript
        {
            get
            {
                if (_animationScript is null)
                {
                    throw new InvalidOperationException();
                }

                return _animationScript;
            }
        }

        public int State => AnimationScript.GetState();
    }
}
