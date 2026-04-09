// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IBadLuckProtectionConfig
    {
        bool NormalizedBonusEnabled { get; }

        bool ResetOnGiftReceived { get; }

        float GetBonusPercentageForRiskLevel(RiskLevel riskLevel);
    }
}
