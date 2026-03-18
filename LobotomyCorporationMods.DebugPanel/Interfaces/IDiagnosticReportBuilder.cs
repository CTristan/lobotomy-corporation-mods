// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IDiagnosticReportBuilder
    {
        DiagnosticReport BuildReport();
    }
}
