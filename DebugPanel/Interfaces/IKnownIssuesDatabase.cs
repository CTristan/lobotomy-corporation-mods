// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.JsonModels;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IKnownIssuesDatabase
    {
        IList<KnownIssueItem> GetKnownIssues();

        string DatabaseVersion { get; }
    }
}
