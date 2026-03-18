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
    public sealed class PatchInfoData
    {
        public string targetType;
        public string targetMethod;
        public string patchType;
        public string owner;
        public string patchMethod;
        public string patchAssemblyName;

        public static PatchInfoData FromModel(PatchInfo model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new PatchInfoData
            {
                targetType = model.TargetType,
                targetMethod = model.TargetMethod,
                patchType = model.PatchType.ToString(),
                owner = model.Owner,
                patchMethod = model.PatchMethod,
                patchAssemblyName = model.PatchAssemblyName,
            };
        }

        public static PatchInfoData[] FromModels(IList<PatchInfo> models)
        {
            ThrowHelper.ThrowIfNull(models);

            var result = new PatchInfoData[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                result[i] = FromModel(models[i]);
            }

            return result;
        }
    }
}
