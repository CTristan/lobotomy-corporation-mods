// SPDX-License-Identifier: MIT

using System.ComponentModel;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts
{
    /// <summary>How the bonus gift chance is calculated per work session.</summary>
    public enum BonusCalculationMode
    {
        /// <summary>Divide success count by max cube count to normalize across abnormalities.</summary>
        [Description("Normalized")]
        Normalized,

        /// <summary>Use raw success count directly per PE box interaction.</summary>
        [Description("Per PE-Box")]
        PerPEBox,
    }
}
