// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IExpectedPatchSource
    {
        IList<ExpectedPatchInfo> GetExpectedPatches(IList<string> debugInfo);
    }
}
