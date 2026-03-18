// SPDX-License-Identifier: MIT

#region

using System;

using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class PatchComparisonData
    {
        public MissingPatchData[] missingPatches;
        public int totalExpected;
        public int totalMatched;
        public bool hasMissingPatches;

        public static PatchComparisonData FromModel(PatchComparisonResult model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new PatchComparisonData
            {
                missingPatches = MissingPatchData.FromModels(model.MissingPatches),
                totalExpected = model.TotalExpected,
                totalMatched = model.TotalMatched,
                hasMissingPatches = model.HasMissingPatches,
            };
        }
    }
}
