// SPDX-License-Identifier: MIT

using System.ComponentModel;
using ConfigurationManager;
using ConfigurationManager.Config;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    /// <summary>Configuration backed by ConfigurationManager for in-game settings UI.</summary>
    public sealed class BadLuckProtectionConfig : IBadLuckProtectionConfig
    {
        private const string ModId = "BadLuckProtectionForGifts";
        private const string ModName = "Bad Luck Protection For Gifts";
        private const string GeneralSection = "General";
        private const string BonusSection = "Gift Chance Bonus";
        private const float DefaultBonusPercentage = 1.0f;

        private readonly LmmConfigEntry<BonusCalculationMode> _bonusCalculationMode;
        private readonly LmmConfigEntry<int> _giftChanceDecimalPlaces;
        private readonly LmmConfigEntry<bool> _resetOnGiftReceived;
        private readonly LmmConfigEntry<float> _zayinBonus;
        private readonly LmmConfigEntry<float> _tethBonus;
        private readonly LmmConfigEntry<float> _heBonus;
        private readonly LmmConfigEntry<float> _wawBonus;
        private readonly LmmConfigEntry<float> _alephBonus;

        public BadLuckProtectionConfig()
        {
            var version = typeof(BadLuckProtectionConfig).Assembly.GetName().Version.ToString(3);
            var configFile = LmmConfigRegistration.GetConfigFile(ModId, ModName, version);

            _bonusCalculationMode = configFile.Bind(
                GeneralSection,
                "BonusCalculationMode",
                BonusCalculationMode.Normalized,
                new LmmConfigDescription(
                    "Normalized: All abnormalities gain bonus at the same rate. The bonus is based on how many PE boxes you filled out of the total.\nExample: filling 5 out of 10 PE boxes counts as 0.5.\n\nPer PE-Box: Abnormalities with more PE boxes gain bonus faster. Each filled PE box adds 1 to the bonus.\nExample: filling 7 PE boxes counts as 7.",
                    null,
                    new DisplayNameAttribute("Bonus Calculation Mode")
                )
            );

            _resetOnGiftReceived = configFile.Bind(
                GeneralSection,
                "ResetOnGiftReceived",
                true,
                new LmmConfigDescription(
                    "When an agent receives a gift, reset their bonus for that gift to zero.",
                    null,
                    new DisplayNameAttribute("Reset On Gift Received")
                )
            );

            _giftChanceDecimalPlaces = configFile.Bind(
                GeneralSection,
                "GiftChanceDecimalPlaces",
                2,
                new LmmConfigDescription(
                    "Number of decimal places shown in the gift chance display.",
                    new AcceptableValueRange<int>(0, 3),
                    new DisplayNameAttribute("Gift Chance Decimal Places")
                )
            );

            _zayinBonus = BindBonusPercentage(configFile, "ZayinBonusPercentage", "ZAYIN", 5);
            _tethBonus = BindBonusPercentage(configFile, "TethBonusPercentage", "TETH", 4);
            _heBonus = BindBonusPercentage(configFile, "HeBonusPercentage", "HE", 3);
            _wawBonus = BindBonusPercentage(configFile, "WawBonusPercentage", "WAW", 2);
            _alephBonus = BindBonusPercentage(configFile, "AlephBonusPercentage", "ALEPH", 1);
        }

        public BonusCalculationMode BonusCalculationMode => _bonusCalculationMode.Value;

        public int GiftChanceDecimalPlaces => _giftChanceDecimalPlaces.Value;

        public bool ResetOnGiftReceived => _resetOnGiftReceived.Value;

        public float GetBonusPercentageForRiskLevel(RiskLevel riskLevel)
        {
            switch (riskLevel)
            {
                case RiskLevel.ZAYIN:
                    return _zayinBonus.Value;
                case RiskLevel.TETH:
                    return _tethBonus.Value;
                case RiskLevel.HE:
                    return _heBonus.Value;
                case RiskLevel.WAW:
                    return _wawBonus.Value;
                case RiskLevel.ALEPH:
                    return _alephBonus.Value;
                default:
                    return DefaultBonusPercentage;
            }
        }

        private static LmmConfigEntry<float> BindBonusPercentage(
            LmmConfigFile configFile,
            string key,
            string riskLevelName,
            int order
        )
        {
            return configFile.Bind(
                BonusSection,
                key,
                DefaultBonusPercentage,
                new LmmConfigDescription(
                    "Extra gift chance (percent) added after each successful work session with "
                        + riskLevelName
                        + " abnormalities.",
                    new AcceptableValueRange<float>(0.0f, 100.0f),
                    new DisplayNameAttribute(riskLevelName + " Bonus Percentage"),
                    new ConfigurationManagerAttributes { Order = order, UseIntegerSlider = true }
                )
            );
        }
    }
}
