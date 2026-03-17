// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.DebugPanel.JsonModels;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IKnownIssuesDatabase
    {
        IList<KnownIssueItem> GetKnownIssues();

        string DatabaseVersion { get; }
    }
}
