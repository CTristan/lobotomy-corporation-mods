// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
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
