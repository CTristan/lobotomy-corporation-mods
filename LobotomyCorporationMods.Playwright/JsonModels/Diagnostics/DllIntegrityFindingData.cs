// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class DllIntegrityFindingData
    {
        public string dllPath;
        public string dllName;
        public string severity;
        public string[] onDiskHarmonyReferences;
        public string[] originalHarmonyReferences;
        public bool hasBackup;
        public string backupPath;
        public bool wasRewritten;
        public string summary;

        public static DllIntegrityFindingData FromModel(DllIntegrityFinding model)
        {
            ThrowHelper.ThrowIfNull(model);

            var onDisk = new string[model.OnDiskHarmonyReferences.Count];
            model.OnDiskHarmonyReferences.CopyTo(onDisk, 0);

            var original = new string[model.OriginalHarmonyReferences.Count];
            model.OriginalHarmonyReferences.CopyTo(original, 0);

            return new DllIntegrityFindingData
            {
                dllPath = model.DllPath,
                dllName = model.DllName,
                severity = model.Severity.ToString(),
                onDiskHarmonyReferences = onDisk,
                originalHarmonyReferences = original,
                hasBackup = model.HasBackup,
                backupPath = model.BackupPath,
                wasRewritten = model.WasRewritten,
                summary = model.Summary,
            };
        }

        public static DllIntegrityFindingData[] FromModels(IList<DllIntegrityFinding> models)
        {
            ThrowHelper.ThrowIfNull(models);

            var result = new DllIntegrityFindingData[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                result[i] = FromModel(models[i]);
            }

            return result;
        }
    }
}
