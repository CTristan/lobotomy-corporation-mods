// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
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
