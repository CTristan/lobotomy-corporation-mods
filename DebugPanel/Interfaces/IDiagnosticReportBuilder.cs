// SPDX-License-Identifier: MIT

#region

using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IDiagnosticReportBuilder
    {
        DiagnosticReport BuildReport();
    }
}
