// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.DebugPanel.JsonModels
{
    [Serializable]
    public class KnownIssuesData
    {
        public string version;
        public string lastUpdated;
        public KnownIssueItem[] issues;

        public string Version => version;

        public string LastUpdated => lastUpdated;

        public KnownIssueItem[] Issues => issues;
    }
}
