// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class RetargetHarmonyStatus
    {
        public RetargetHarmonyStatus(bool isDetected, bool assemblyCSharpRetargeted, bool lobotomyBaseModLibRetargeted, string message)
        {
            IsDetected = isDetected;
            AssemblyCSharpRetargeted = assemblyCSharpRetargeted;
            LobotomyBaseModLibRetargeted = lobotomyBaseModLibRetargeted;
            Message = message ?? string.Empty;
        }

        public bool IsDetected { get; private set; }

        public bool AssemblyCSharpRetargeted { get; private set; }

        public bool LobotomyBaseModLibRetargeted { get; private set; }

        public string Message { get; private set; }
    }
}
