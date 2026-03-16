// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Models.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IEnvironmentDetector
    {
        bool IsHarmony2Available { get; }

        bool IsBepInExAvailable { get; }

        bool IsMonoCecilAvailable { get; }

        EnvironmentInfo DetectEnvironment();
    }
}
