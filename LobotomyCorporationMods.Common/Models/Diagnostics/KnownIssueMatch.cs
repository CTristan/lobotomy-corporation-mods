// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class KnownIssueMatch
    {
        public KnownIssueMatch(
            string modName,
            FindingSeverity severity,
            string description,
            string fixSuggestion,
            string wikiLink,
            string matchedBy)
        {
            ThrowHelper.ThrowIfNull(modName);
            ModName = modName;
            Severity = severity;
            ThrowHelper.ThrowIfNull(description);
            Description = description;
            ThrowHelper.ThrowIfNull(fixSuggestion);
            FixSuggestion = fixSuggestion;
            ThrowHelper.ThrowIfNull(wikiLink);
            WikiLink = wikiLink;
            ThrowHelper.ThrowIfNull(matchedBy);
            MatchedBy = matchedBy;
        }

        public string ModName { get; private set; }

        public FindingSeverity Severity { get; private set; }

        public string Description { get; private set; }

        public string FixSuggestion { get; private set; }

        public string WikiLink { get; private set; }

        public string MatchedBy { get; private set; }
    }
}
