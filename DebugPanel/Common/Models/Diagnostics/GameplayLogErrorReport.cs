// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class GameplayLogErrorReport
    {
        public GameplayLogErrorReport(IList<GameplayLogErrorEntry> entries)
        {
            ThrowHelper.ThrowIfNull(entries);
            Entries = entries;
        }

        public IList<GameplayLogErrorEntry> Entries { get; private set; }
    }
}
