// SPDX-License-Identifier: MIT

namespace HarmonyDebugPanel.Models
{
    public sealed class RetargetHarmonyStatus(bool isDetected, bool assemblyCSharpRetargeted, bool lobotomyBaseModLibRetargeted, string message)
    {
        public RetargetHarmonyStatus()
            : this(false, false, false, "Not detected")
        {
        }

        public bool IsDetected { get; private set; } = isDetected;

        public bool AssemblyCSharpRetargeted { get; private set; } = assemblyCSharpRetargeted;

        public bool LobotomyBaseModLibRetargeted { get; private set; } = lobotomyBaseModLibRetargeted;

        public string Message { get; private set; } = message ?? string.Empty;
    }
}
