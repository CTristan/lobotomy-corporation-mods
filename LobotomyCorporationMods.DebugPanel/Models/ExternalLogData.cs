// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class ExternalLogData
    {
        public ExternalLogData(string retargetHarmonyLog, string bepInExLog, string unityLog)
        {
            RetargetHarmonyLog = Guard.Against.Null(retargetHarmonyLog, nameof(retargetHarmonyLog));
            BepInExLog = Guard.Against.Null(bepInExLog, nameof(bepInExLog));
            UnityLog = Guard.Against.Null(unityLog, nameof(unityLog));
        }

        public string RetargetHarmonyLog { get; private set; }

        public string BepInExLog { get; private set; }

        public string UnityLog { get; private set; }
    }
}
