// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface ILogFileWriter
    {
        void WriteReport(DiagnosticReport report);
    }
}
