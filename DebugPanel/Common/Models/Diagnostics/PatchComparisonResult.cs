// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class PatchComparisonResult
    {
        public PatchComparisonResult(IList<MissingPatchInfo> missingPatches, int totalExpected, int totalMatched)
        {
            ThrowHelper.ThrowIfNull(missingPatches);
            MissingPatches = missingPatches;
            TotalExpected = totalExpected;
            TotalMatched = totalMatched;
        }

        public IList<MissingPatchInfo> MissingPatches { get; private set; }

        public int TotalExpected { get; private set; }

        public int TotalMatched { get; private set; }

        public bool HasMissingPatches => MissingPatches.Count > 0;
    }
}
