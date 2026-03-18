// SPDX-License-Identifier: MIT

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class EnvironmentInfo
    {
        public EnvironmentInfo(bool isHarmony2Available, bool isBepInExAvailable, bool isMonoCecilAvailable)
        {
            IsHarmony2Available = isHarmony2Available;
            IsBepInExAvailable = isBepInExAvailable;
            IsMonoCecilAvailable = isMonoCecilAvailable;
        }

        public bool IsHarmony2Available { get; private set; }

        public bool IsBepInExAvailable { get; private set; }

        public bool IsMonoCecilAvailable { get; private set; }
    }
}
