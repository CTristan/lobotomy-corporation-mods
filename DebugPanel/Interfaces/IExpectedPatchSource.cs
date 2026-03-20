// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IExpectedPatchSource
    {
        IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo);
    }
}
