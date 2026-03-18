// SPDX-License-Identifier: MIT

#region

using System;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class DllIntegrityReportData
    {
        public DllIntegrityFindingData[] findings;
        public bool shimBackupDirectoryExists;
        public string shimBackupDirectoryPath;
        public bool interopCacheExists;
        public string interopCachePath;
        public int interopCacheEntryCount;
        public bool monoCecilAvailable;
        public int totalRewrittenCount;
        public string[] warnings;
        public string summary;

        public static DllIntegrityReportData FromModel(DllIntegrityReport model)
        {
            ThrowHelper.ThrowIfNull(model);

            var warningArr = new string[model.Warnings.Count];
            model.Warnings.CopyTo(warningArr, 0);

            return new DllIntegrityReportData
            {
                findings = DllIntegrityFindingData.FromModels(model.Findings),
                shimBackupDirectoryExists = model.ShimBackupDirectoryExists,
                shimBackupDirectoryPath = model.ShimBackupDirectoryPath,
                interopCacheExists = model.InteropCacheExists,
                interopCachePath = model.InteropCachePath,
                interopCacheEntryCount = model.InteropCacheEntryCount,
                monoCecilAvailable = model.MonoCecilAvailable,
                totalRewrittenCount = model.TotalRewrittenCount,
                warnings = warningArr,
                summary = model.Summary,
            };
        }
    }
}
