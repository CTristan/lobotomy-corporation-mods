// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IIssuesAggregator
    {
        IList<DiagnosticIssue> AggregateIssues(DiagnosticReport report);
    }
}
