// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    /// <summary>Fallback configuration used when ConfigurationManager is not installed.</summary>
    public sealed class DefaultBadLuckProtectionConfig : IBadLuckProtectionConfig
    {
        private const float DefaultBonusPercentage = 1.0f;

        public bool NormalizedBonusEnabled => true;

        public bool ResetOnGiftReceived => false;

        public float GetBonusPercentageForRiskLevel(RiskLevel riskLevel)
        {
            return DefaultBonusPercentage;
        }
    }
}
