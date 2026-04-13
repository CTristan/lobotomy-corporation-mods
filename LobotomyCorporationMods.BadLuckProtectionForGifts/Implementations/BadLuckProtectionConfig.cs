// SPDX-License-Identifier: MIT

using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Interfaces;

namespace LobotomyCorporationMods.BadLuckProtectionForGifts.Implementations
{
    /// <summary>Configuration backed by Common's configuration abstractions.</summary>
    public sealed class BadLuckProtectionConfig : IBadLuckProtectionConfig
    {
        private const string ModId = "BadLuckProtectionForGifts";
        private const string ModName = "Bad Luck Protection For Gifts";
        private const string GeneralSection = "General";
        private const string BonusSection = "Gift Chance Bonus";
        private const float DefaultBonusPercentage = 1.0f;

        private readonly IConfigEntry<BonusCalculationMode> _bonusCalculationMode;
        private readonly IConfigEntry<int> _giftChanceDecimalPlaces;
        private readonly IConfigEntry<bool> _resetOnGiftReceived;
        private readonly IConfigEntry<bool> _showBaseChance;
        private readonly IConfigEntry<float> _zayinBonus;
        private readonly IConfigEntry<float> _tethBonus;
        private readonly IConfigEntry<float> _heBonus;
        private readonly IConfigEntry<float> _wawBonus;
        private readonly IConfigEntry<float> _alephBonus;

        public BadLuckProtectionConfig()
        {
            var version = typeof(BadLuckProtectionConfig).Assembly.GetName().Version.ToString(3);
            var config = new ModConfig(ModId, ModName, version);

            _bonusCalculationMode = config.Bind(
                GeneralSection,
                "BonusCalculationMode",
                BonusCalculationMode.Normalized,
                "Normalized: All abnormalities gain bonus at the same rate. The bonus is based on how many PE boxes you filled out of the total.\nExample: filling 5 out of 10 PE boxes counts as 0.5.\n\nPer PE-Box: Abnormalities with more PE boxes gain bonus faster. Each filled PE box adds 1 to the bonus.\nExample: filling 7 PE boxes counts as 7.",
                displayName: "Bonus Calculation Mode"
            );

            _resetOnGiftReceived = config.Bind(
                GeneralSection,
                "ResetOnGiftReceived",
                true,
                "When an agent receives a gift, reset their bonus for that gift to zero.",
                displayName: "Reset On Gift Received"
            );

            _giftChanceDecimalPlaces = config.Bind(
                GeneralSection,
                "GiftChanceDecimalPlaces",
                2,
                "Number of decimal places shown in the gift chance display.",
                range: new AcceptableValueRange<int>(0, 3),
                displayName: "Gift Chance Decimal Places"
            );

            _showBaseChance = config.Bind(
                GeneralSection,
                "ShowBaseChance",
                true,
                "Show the base gift chance alongside the boosted chance in the UI.",
                displayName: "Show Base Chance"
            );

            _zayinBonus = BindBonusPercentage(config, "ZayinBonusPercentage", "ZAYIN");
            _tethBonus = BindBonusPercentage(config, "TethBonusPercentage", "TETH");
            _heBonus = BindBonusPercentage(config, "HeBonusPercentage", "HE");
            _wawBonus = BindBonusPercentage(config, "WawBonusPercentage", "WAW");
            _alephBonus = BindBonusPercentage(config, "AlephBonusPercentage", "ALEPH");
        }

        public BonusCalculationMode BonusCalculationMode => _bonusCalculationMode.Value;

        public int GiftChanceDecimalPlaces => _giftChanceDecimalPlaces.Value;

        public bool ResetOnGiftReceived => _resetOnGiftReceived.Value;

        public bool ShowBaseChance => _showBaseChance.Value;

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

        private static IConfigEntry<float> BindBonusPercentage(
            ModConfig config,
            string key,
            string riskLevelName
        )
        {
            return config.Bind(
                BonusSection,
                key,
                DefaultBonusPercentage,
                "Extra gift chance (percent) added after each successful work session with "
                    + riskLevelName
                    + " abnormalities.",
                range: new AcceptableValueRange<float>(0.0f, 100.0f),
                displayName: riskLevelName + " Bonus Percentage",
                useSlider: true
            );
        }
    }
}
