// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class DllIntegrityFinding
    {
        public DllIntegrityFinding(
            string dllPath,
            string dllName,
            FindingSeverity severity,
            IList<string> onDiskHarmonyReferences,
            IList<string> originalHarmonyReferences,
            bool hasBackup,
            string backupPath,
            bool wasRewritten,
            string summary)
        {
            ThrowHelper.ThrowIfNull(dllPath);
            DllPath = dllPath;
            ThrowHelper.ThrowIfNull(dllName);
            DllName = dllName;
            Severity = severity;
            ThrowHelper.ThrowIfNull(onDiskHarmonyReferences);
            OnDiskHarmonyReferences = onDiskHarmonyReferences;
            ThrowHelper.ThrowIfNull(originalHarmonyReferences);
            OriginalHarmonyReferences = originalHarmonyReferences;
            HasBackup = hasBackup;
            BackupPath = backupPath ?? string.Empty;
            WasRewritten = wasRewritten;
            ThrowHelper.ThrowIfNull(summary);
            Summary = summary;
        }

        public string DllPath { get; private set; }

        public string DllName { get; private set; }

        public FindingSeverity Severity { get; private set; }

        public IList<string> OnDiskHarmonyReferences { get; private set; }

        public IList<string> OriginalHarmonyReferences { get; private set; }

        public bool HasBackup { get; private set; }

        public string BackupPath { get; private set; }

        public bool WasRewritten { get; private set; }

        public string Summary { get; private set; }
    }
}
