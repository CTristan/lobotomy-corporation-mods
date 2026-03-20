// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class ErrorLogReport
    {
        public ErrorLogReport(IList<ErrorLogEntry> entries)
        {
            ThrowHelper.ThrowIfNull(entries);
            Entries = entries;
        }

        public IList<ErrorLogEntry> Entries { get; private set; }
    }
}
