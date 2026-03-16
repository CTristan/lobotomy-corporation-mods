// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.Playwright.JsonModels.Diagnostics
{
    [Serializable]
    public sealed class ExternalLogDataModel
    {
        public string retargetHarmonyLog;
        public string bepInExLog;
        public string unityLog;

        public static ExternalLogDataModel FromModel(ExternalLogData model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new ExternalLogDataModel
            {
                retargetHarmonyLog = model.RetargetHarmonyLog,
                bepInExLog = model.BepInExLog,
                unityLog = model.UnityLog,
            };
        }
    }
}
