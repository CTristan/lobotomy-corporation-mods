// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class ExternalLogData
    {
        public ExternalLogData(string retargetHarmonyLog, string bepInExLog, string unityLog)
        {
            ThrowHelper.ThrowIfNull(retargetHarmonyLog);
            RetargetHarmonyLog = retargetHarmonyLog;
            ThrowHelper.ThrowIfNull(bepInExLog);
            BepInExLog = bepInExLog;
            ThrowHelper.ThrowIfNull(unityLog);
            UnityLog = unityLog;
        }

        public string RetargetHarmonyLog { get; private set; }

        public string BepInExLog { get; private set; }

        public string UnityLog { get; private set; }
    }
}
