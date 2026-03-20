// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace DebugPanel.JsonModels
{
    [Serializable]
    public class KnownIssueItem
    {
        public string modName;
        public string dllName;
        public string assemblyName;
        public int severity;
        public string description;
        public string fixSuggestion;
        public string wikiLink;
        public string[] conflictsWith;

        public string ModName => modName;

        public string DllName => dllName;

        public string AssemblyName => assemblyName;

        public int Severity => severity;

        public string Description => description;

        public string FixSuggestion => fixSuggestion;

        public string WikiLink => wikiLink;

        public string[] ConflictsWith => conflictsWith;
    }
}
