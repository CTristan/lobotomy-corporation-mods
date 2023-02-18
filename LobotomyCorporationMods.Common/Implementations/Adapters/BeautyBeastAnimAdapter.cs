// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class BeautyBeastAnimAdapter : IBeautyBeastAnimAdapter
    {
        private readonly BeautyBeastAnim _animationScript;

        public BeautyBeastAnimAdapter(BeautyBeastAnim animationScript)
        {
            _animationScript = animationScript;
        }

        public int GetState()
        {
            return _animationScript.GetState();
        }
    }
}
