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
    public sealed class DetectedModData
    {
        public string name;
        public string version;
        public string source;
        public string harmonyVersion;
        public string assemblyName;
        public string identifier;
        public bool hasActivePatches;
        public int activePatchCount;
        public int expectedPatchCount;

        public static DetectedModData FromModel(DetectedModInfo model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new DetectedModData
            {
                name = model.Name,
                version = model.Version,
                source = model.Source.ToString(),
                harmonyVersion = model.HarmonyVersion.ToString(),
                assemblyName = model.AssemblyName,
                identifier = model.Identifier,
                hasActivePatches = model.HasActivePatches,
                activePatchCount = model.ActivePatchCount,
                expectedPatchCount = model.ExpectedPatchCount,
            };
        }

        public static DetectedModData[] FromModels(IList<DetectedModInfo> models)
        {
            ThrowHelper.ThrowIfNull(models);

            var result = new DetectedModData[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                result[i] = FromModel(models[i]);
            }

            return result;
        }
    }
}
