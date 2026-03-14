// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
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
            DllPath = Guard.Against.Null(dllPath, nameof(dllPath));
            DllName = Guard.Against.Null(dllName, nameof(dllName));
            Severity = severity;
            OnDiskHarmonyReferences = Guard.Against.Null(onDiskHarmonyReferences, nameof(onDiskHarmonyReferences));
            OriginalHarmonyReferences = Guard.Against.Null(originalHarmonyReferences, nameof(originalHarmonyReferences));
            HasBackup = hasBackup;
            BackupPath = backupPath ?? string.Empty;
            WasRewritten = wasRewritten;
            Summary = Guard.Against.Null(summary, nameof(summary));
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
