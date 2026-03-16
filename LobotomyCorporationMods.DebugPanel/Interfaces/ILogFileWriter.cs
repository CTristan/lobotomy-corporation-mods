// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface ILogFileWriter
    {
        string WriteReport(DiagnosticReport report);
    }
}
