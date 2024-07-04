// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class BeautyBeastAnimTestAdapter : Adapter<BeautyBeastAnim>, IBeautyBeastAnimTestAdapter
    {
        internal BeautyBeastAnimTestAdapter([NotNull] BeautyBeastAnim beautyBeastAnim)
        {
            GameObject = beautyBeastAnim;
        }

        public int State => GameObject.GetState();
    }
}
