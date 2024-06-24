// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class BeautyBeastAnimAdapter : Adapter<BeautyBeastAnim>, IBeautyBeastAnimAdapter
    {
        public int State => GameObject.GetState();
    }
}
