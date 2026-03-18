// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IReportFormatter
    {
        IList<string> FormatForOverlay(DiagnosticReport report);

        IList<string> FormatForLogFile(DiagnosticReport report, ExternalLogData externalLogs);
    }
}
