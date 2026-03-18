// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.DebugPanel.JsonModels;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IKnownIssuesDatabase
    {
        IList<KnownIssueItem> GetKnownIssues();

        string DatabaseVersion { get; }
    }
}
