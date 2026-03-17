// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class ExternalLogData
    {
        public ExternalLogData(
            string retargetHarmonyLog,
            string bepInExLog,
            string unityLog,
            string gameplayLog,
            string saveFolderLog,
            string lmmDirectoryLog,
            string lmmSystemLog,
            string baseModsLog)
        {
            ThrowHelper.ThrowIfNull(retargetHarmonyLog);
            RetargetHarmonyLog = retargetHarmonyLog;
            ThrowHelper.ThrowIfNull(bepInExLog);
            BepInExLog = bepInExLog;
            ThrowHelper.ThrowIfNull(unityLog);
            UnityLog = unityLog;
            ThrowHelper.ThrowIfNull(gameplayLog);
            GameplayLog = gameplayLog;
            ThrowHelper.ThrowIfNull(saveFolderLog);
            SaveFolderLog = saveFolderLog;
            ThrowHelper.ThrowIfNull(lmmDirectoryLog);
            LmmDirectoryLog = lmmDirectoryLog;
            ThrowHelper.ThrowIfNull(lmmSystemLog);
            LmmSystemLog = lmmSystemLog;
            ThrowHelper.ThrowIfNull(baseModsLog);
            BaseModsLog = baseModsLog;
        }

        public string RetargetHarmonyLog { get; private set; }

        public string BepInExLog { get; private set; }

        public string UnityLog { get; private set; }

        public string GameplayLog { get; private set; }

        public string SaveFolderLog { get; private set; }

        public string LmmDirectoryLog { get; private set; }

        public string LmmSystemLog { get; private set; }

        public string BaseModsLog { get; private set; }
    }
}
