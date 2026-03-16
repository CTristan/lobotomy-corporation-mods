// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IExternalLogSource
    {
        ExternalLogData GetExternalLogs();
    }
}
