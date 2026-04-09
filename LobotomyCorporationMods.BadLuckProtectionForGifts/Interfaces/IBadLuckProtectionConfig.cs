// SPDX-License-Identifier: MIT

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces
{
    public interface IBadLuckProtectionConfig
    {
        BonusCalculationMode BonusCalculationMode { get; }

        int GiftChanceDecimalPlaces { get; }

        bool ResetOnGiftReceived { get; }

        float GetBonusPercentageForRiskLevel(RiskLevel riskLevel);
    }
}
