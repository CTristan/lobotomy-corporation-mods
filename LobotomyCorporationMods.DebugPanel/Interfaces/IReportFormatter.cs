// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IReportFormatter
    {
        IList<string> FormatForOverlay(DiagnosticReport report);

        IList<string> FormatForLogFile(DiagnosticReport report, ExternalLogData externalLogs);
    }
}
