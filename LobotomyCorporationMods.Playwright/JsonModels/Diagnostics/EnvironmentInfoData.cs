// SPDX-License-Identifier: MIT

#region

using System;
using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class EnvironmentInfoData
    {
        public bool isHarmony2Available;
        public bool isBepInExAvailable;
        public bool isMonoCecilAvailable;

        public static EnvironmentInfoData FromModel(EnvironmentInfo model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new EnvironmentInfoData
            {
                isHarmony2Available = model.IsHarmony2Available,
                isBepInExAvailable = model.IsBepInExAvailable,
                isMonoCecilAvailable = model.IsMonoCecilAvailable,
            };
        }
    }
}
