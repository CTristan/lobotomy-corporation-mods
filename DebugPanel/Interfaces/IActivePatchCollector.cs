// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IActivePatchCollector : IInfoCollector<IList<PatchInfo>>
    {
    }
}
