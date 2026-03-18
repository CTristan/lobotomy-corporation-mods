// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IYggdrasilAnimTestAdapter : IComponentTestAdapter<YggdrasilAnim>
    {
        IEnumerable<IGameObjectTestAdapter> Flowers { get; }
    }
}
