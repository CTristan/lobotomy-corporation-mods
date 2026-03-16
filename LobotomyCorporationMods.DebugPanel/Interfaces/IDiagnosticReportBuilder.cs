// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IDiagnosticReportBuilder
    {
        DiagnosticReport BuildReport();
    }
}
