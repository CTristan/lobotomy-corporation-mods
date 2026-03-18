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
    public sealed class MissingPatchData
    {
        public string patchAssembly;
        public string targetType;
        public string targetMethod;
        public string patchMethod;
        public string patchType;

        public static MissingPatchData FromModel(MissingPatchInfo model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new MissingPatchData
            {
                patchAssembly = model.PatchAssembly,
                targetType = model.TargetType,
                targetMethod = model.TargetMethod,
                patchMethod = model.PatchMethod,
                patchType = model.PatchType.ToString(),
            };
        }

        public static MissingPatchData[] FromModels(IList<MissingPatchInfo> models)
        {
            ThrowHelper.ThrowIfNull(models);

            var result = new MissingPatchData[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                result[i] = FromModel(models[i]);
            }

            return result;
        }
    }
}
