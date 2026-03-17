// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class FilesystemValidationReport
    {
        public FilesystemValidationReport(IList<DiagnosticIssue> issues, string summary)
        {
            ThrowHelper.ThrowIfNull(issues);
            Issues = issues;
            ThrowHelper.ThrowIfNull(summary);
            Summary = summary;
        }

        public IList<DiagnosticIssue> Issues { get; private set; }

        public string Summary { get; private set; }
    }
}
