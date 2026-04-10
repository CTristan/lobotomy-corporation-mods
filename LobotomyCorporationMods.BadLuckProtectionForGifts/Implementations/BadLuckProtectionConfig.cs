// SPDX-License-Identifier: MIT

using LobotomyCorporation.Mods.Common.Constants;
using LobotomyCorporation.Mods.Common.Implementations;
using LobotomyCorporation.Mods.Common.Interfaces;
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

        private readonly IConfigurationEntry<BonusCalculationMode> _bonusCalculationMode;
        private readonly IConfigurationEntry<int> _giftChanceDecimalPlaces;
        private readonly IConfigurationEntry<bool> _resetOnGiftReceived;
        private readonly IConfigurationEntry<bool> _showBaseChance;
        private readonly IConfigurationEntry<float> _zayinBonus;
        private readonly IConfigurationEntry<float> _tethBonus;
        private readonly IConfigurationEntry<float> _heBonus;
        private readonly IConfigurationEntry<float> _wawBonus;
        private readonly IConfigurationEntry<float> _alephBonus;

        public BadLuckProtectionConfig()
        {
            var version = typeof(BadLuckProtectionConfig).Assembly.GetName().Version.ToString(3);

            _bonusCalculationMode = ConfigurationEntryBuilder
                .ForMod(ModId, ModName, version)
                .InSection(GeneralSection)
                .WithKey("BonusCalculationMode")
                .WithDefault(BonusCalculationMode.Normalized)
                .WithDisplayName("Bonus Calculation Mode")
                .WithDescription(
                    "Normalized: All abnormalities gain bonus at the same rate. The bonus is based on how many PE boxes you filled out of the total.\nExample: filling 5 out of 10 PE boxes counts as 0.5.\n\nPer PE-Box: Abnormalities with more PE boxes gain bonus faster. Each filled PE box adds 1 to the bonus.\nExample: filling 7 PE boxes counts as 7."
                )
                .Register<BonusCalculationMode>();

            _resetOnGiftReceived = ConfigurationEntryBuilder
                .ForMod(ModId, ModName, version)
                .InSection(GeneralSection)
                .WithKey("ResetOnGiftReceived")
                .WithDefault(true)
                .WithDisplayName("Reset On Gift Received")
                .WithDescription(
                    "When an agent receives a gift, reset their bonus for that gift to zero."
                )
                .Register<bool>();

            _giftChanceDecimalPlaces = ConfigurationEntryBuilder
                .ForMod(ModId, ModName, version)
                .InSection(GeneralSection)
                .WithKey("GiftChanceDecimalPlaces")
                .WithDefault(2)
                .WithDisplayName("Gift Chance Decimal Places")
                .WithDescription("Number of decimal places shown in the gift chance display.")
                .WithRange(0, 3)
                .Register<int>();

            _showBaseChance = ConfigurationEntryBuilder
                .ForMod(ModId, ModName, version)
                .InSection(GeneralSection)
                .WithKey("ShowBaseChance")
                .WithDefault(true)
                .WithDisplayName("Show Base Chance")
                .WithDescription(
                    "Show the base gift chance alongside the boosted chance in the UI."
                )
                .Register<bool>();

            _zayinBonus = BindBonusPercentage(version, "ZayinBonusPercentage", "ZAYIN", 5);
            _tethBonus = BindBonusPercentage(version, "TethBonusPercentage", "TETH", 4);
            _heBonus = BindBonusPercentage(version, "HeBonusPercentage", "HE", 3);
            _wawBonus = BindBonusPercentage(version, "WawBonusPercentage", "WAW", 2);
            _alephBonus = BindBonusPercentage(version, "AlephBonusPercentage", "ALEPH", 1);
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

        private static IConfigurationEntry<float> BindBonusPercentage(
            string version,
            string key,
            string riskLevelName,
            int order
        )
        {
            return ConfigurationEntryBuilder
                .ForMod(ModId, ModName, version)
                .InSection(BonusSection)
                .WithKey(key)
                .WithDefault(DefaultBonusPercentage)
                .WithDisplayName(riskLevelName + " Bonus Percentage")
                .WithDescription(
                    "Extra gift chance (percent) added after each successful work session with "
                        + riskLevelName
                        + " abnormalities."
                )
                .WithRange(0.0f, 100.0f)
                .WithHint(ConfigurationHints.UseIntegerSlider, true)
                .WithOrder(order)
                .Register<float>();
        }
    }
}
