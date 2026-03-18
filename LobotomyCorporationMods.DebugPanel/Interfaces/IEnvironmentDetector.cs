// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IEnvironmentDetector
    {
        bool IsHarmony2Available { get; }

        bool IsBepInExAvailable { get; }

        bool IsMonoCecilAvailable { get; }

        EnvironmentInfo DetectEnvironment();
    }
}
