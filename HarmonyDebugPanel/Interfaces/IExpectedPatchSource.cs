// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Interfaces
{
    public interface IExpectedPatchSource
    {
        IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo);
    }
}
