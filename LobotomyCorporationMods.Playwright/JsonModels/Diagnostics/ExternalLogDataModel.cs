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
        public string gameplayLog;
        public string saveFolderLog;
        public string lmmDirectoryLog;
        public string lmmSystemLog;
        public string baseModsLog;

        public static ExternalLogDataModel FromModel(ExternalLogData model)
        {
            ThrowHelper.ThrowIfNull(model);

            return new ExternalLogDataModel
            {
                retargetHarmonyLog = model.RetargetHarmonyLog,
                bepInExLog = model.BepInExLog,
                unityLog = model.UnityLog,
                gameplayLog = model.GameplayLog,
                saveFolderLog = model.SaveFolderLog,
                lmmDirectoryLog = model.LmmDirectoryLog,
                lmmSystemLog = model.LmmSystemLog,
                baseModsLog = model.BaseModsLog,
            };
        }
    }
}
