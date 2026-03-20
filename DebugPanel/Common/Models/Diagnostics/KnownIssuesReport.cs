// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class KnownIssuesReport
    {
        public KnownIssuesReport(IList<KnownIssueMatch> matches, string databaseVersion)
        {
            ThrowHelper.ThrowIfNull(matches);
            Matches = matches;
            ThrowHelper.ThrowIfNull(databaseVersion);
            DatabaseVersion = databaseVersion;
        }

        public IList<KnownIssueMatch> Matches { get; private set; }

        public string DatabaseVersion { get; private set; }
    }
}
