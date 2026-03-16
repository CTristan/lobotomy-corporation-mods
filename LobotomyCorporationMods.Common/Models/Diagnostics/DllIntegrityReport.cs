// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class DllIntegrityReport
    {
        public DllIntegrityReport(
            IList<DllIntegrityFinding> findings,
            bool shimBackupDirectoryExists,
            string shimBackupDirectoryPath,
            bool interopCacheExists,
            string interopCachePath,
            int interopCacheEntryCount,
            bool monoCecilAvailable,
            int totalRewrittenCount,
            IList<string> warnings,
            string summary)
        {
            ThrowHelper.ThrowIfNull(findings);
            Findings = findings;
            ShimBackupDirectoryExists = shimBackupDirectoryExists;
            ShimBackupDirectoryPath = shimBackupDirectoryPath ?? string.Empty;
            InteropCacheExists = interopCacheExists;
            InteropCachePath = interopCachePath ?? string.Empty;
            InteropCacheEntryCount = interopCacheEntryCount;
            MonoCecilAvailable = monoCecilAvailable;
            TotalRewrittenCount = totalRewrittenCount;
            ThrowHelper.ThrowIfNull(warnings);
            Warnings = warnings;
            ThrowHelper.ThrowIfNull(summary);
            Summary = summary;
        }

        public IList<DllIntegrityFinding> Findings { get; private set; }

        public bool ShimBackupDirectoryExists { get; private set; }

        public string ShimBackupDirectoryPath { get; private set; }

        public bool InteropCacheExists { get; private set; }

        public string InteropCachePath { get; private set; }

        public int InteropCacheEntryCount { get; private set; }

        public bool MonoCecilAvailable { get; private set; }

        public int TotalRewrittenCount { get; private set; }

        public IList<string> Warnings { get; private set; }

        public string Summary { get; private set; }
    }
}
