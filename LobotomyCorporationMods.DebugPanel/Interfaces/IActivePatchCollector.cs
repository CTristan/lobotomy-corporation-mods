// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IActivePatchCollector : IInfoCollector<IList<PatchInfo>>
    {
    }
}
