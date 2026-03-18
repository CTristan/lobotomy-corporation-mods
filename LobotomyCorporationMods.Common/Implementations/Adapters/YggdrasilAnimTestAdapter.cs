// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class YggdrasilAnimTestAdapter : ComponentTestAdapter<YggdrasilAnim>, IYggdrasilAnimTestAdapter
    {
        internal YggdrasilAnimTestAdapter([NotNull] YggdrasilAnim gameObject) : base(gameObject)
        {
        }

        [NotNull]
        public IEnumerable<IGameObjectTestAdapter> Flowers
        {
            get
            {
                var flowers = GameObjectInternal.flowers;

                return flowers.Select(flower => new GameObjectTestAdapter(flower)).Cast<IGameObjectTestAdapter>();
            }
        }
    }
}
