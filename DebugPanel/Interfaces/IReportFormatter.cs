// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IReportFormatter
    {
        IList<string> FormatForOverlay(DiagnosticReport report);

        IList<string> FormatForLogFile(DiagnosticReport report, ExternalLogData externalLogs);
    }
}
