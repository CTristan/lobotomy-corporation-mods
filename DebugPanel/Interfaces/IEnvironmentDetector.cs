// SPDX-License-Identifier: MIT

#region

using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IEnvironmentDetector
    {
        bool IsHarmony2Available { get; }

        bool IsBepInExAvailable { get; }

        bool IsMonoCecilAvailable { get; }

        EnvironmentInfo DetectEnvironment();
    }
}
