// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class DiagnosticIssue
    {
        public DiagnosticIssue(
            FindingSeverity severity,
            string category,
            string description,
            string sourceTab,
            string fixSuggestion)
        {
            Severity = severity;
            ThrowHelper.ThrowIfNull(category);
            Category = category;
            ThrowHelper.ThrowIfNull(description);
            Description = description;
            ThrowHelper.ThrowIfNull(sourceTab);
            SourceTab = sourceTab;
            ThrowHelper.ThrowIfNull(fixSuggestion);
            FixSuggestion = fixSuggestion;
        }

        public FindingSeverity Severity { get; private set; }

        public string Category { get; private set; }

        public string Description { get; private set; }

        public string SourceTab { get; private set; }

        public string FixSuggestion { get; private set; }
    }
}
