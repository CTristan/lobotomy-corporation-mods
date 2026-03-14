// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DebugPanel.Models;

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
