// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class BeautyBeastAnimTestAdapter : ComponentTestAdapter<BeautyBeastAnim>, IBeautyBeastAnimTestAdapter
    {
        internal BeautyBeastAnimTestAdapter([NotNull] BeautyBeastAnim gameObject) : base(gameObject)
        {
        }

        public int State => GameObjectInternal.GetState();
    }
}
