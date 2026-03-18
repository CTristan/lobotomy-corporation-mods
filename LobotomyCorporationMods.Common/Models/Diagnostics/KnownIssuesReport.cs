// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
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
