// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class PatchComparisonResult
    {
        public PatchComparisonResult(IList<MissingPatchInfo> missingPatches, int totalExpected, int totalMatched)
        {
            MissingPatches = Guard.Against.Null(missingPatches, nameof(missingPatches));
            TotalExpected = totalExpected;
            TotalMatched = totalMatched;
        }

        public IList<MissingPatchInfo> MissingPatches { get; private set; }

        public int TotalExpected { get; private set; }

        public int TotalMatched { get; private set; }

        public bool HasMissingPatches => MissingPatches.Count > 0;
    }
}
