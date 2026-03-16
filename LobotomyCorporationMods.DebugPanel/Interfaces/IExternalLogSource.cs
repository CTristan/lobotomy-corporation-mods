// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IExternalLogSource
    {
        ExternalLogData GetExternalLogs();
    }
}
