// SPDX-License-Identifier: MIT

#region

using System;

using Hemocode.Common.Implementations;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class RetargetHarmonyData
    {
        public bool isDetected;
        public bool assemblyCSharpRetargeted;
        public bool lobotomyBaseModLibRetargeted;
        public string message;

        public static RetargetHarmonyData FromModel(RetargetHarmonyStatus model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new RetargetHarmonyData
            {
                isDetected = model.IsDetected,
                assemblyCSharpRetargeted = model.AssemblyCSharpRetargeted,
                lobotomyBaseModLibRetargeted = model.LobotomyBaseModLibRetargeted,
                message = model.Message,
            };
        }
    }
}
