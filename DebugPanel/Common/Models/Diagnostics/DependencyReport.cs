// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class DependencyReport
    {
        public DependencyReport(IList<DiagnosticIssue> issues, string baseModVersion, bool baseModListExists)
        {
            ThrowHelper.ThrowIfNull(issues);
            Issues = issues;
            ThrowHelper.ThrowIfNull(baseModVersion);
            BaseModVersion = baseModVersion;
            BaseModListExists = baseModListExists;
        }

        public IList<DiagnosticIssue> Issues { get; private set; }

        public string BaseModVersion { get; private set; }

        public bool BaseModListExists { get; private set; }
    }
}
