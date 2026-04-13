// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Globalization;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Constants;
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

            var generalSectionDisplayName = LocalizationIds.SectionGeneral.GetLocalized();

            _bonusCalculationMode = config.Bind(
                GeneralSection,
                "BonusCalculationMode",
                BonusCalculationMode.Normalized,
                LocalizationIds.DescBonusCalculationMode.GetLocalized(),
                displayName: LocalizationIds.DisplayBonusCalculationMode.GetLocalized(),
                sectionDisplayName: generalSectionDisplayName,
                valueDisplayNames: new Dictionary<string, string>
                {
                    {
                        nameof(BonusCalculationMode.Normalized),
                        LocalizationIds.EnumNormalized.GetLocalized()
                    },
                    {
                        nameof(BonusCalculationMode.PerPEBox),
                        LocalizationIds.EnumPerPEBox.GetLocalized()
                    },
                }
            );

            _resetOnGiftReceived = config.Bind(
                GeneralSection,
                "ResetOnGiftReceived",
                true,
                LocalizationIds.DescResetOnGiftReceived.GetLocalized(),
                displayName: LocalizationIds.DisplayResetOnGiftReceived.GetLocalized(),
                sectionDisplayName: generalSectionDisplayName
            );

            _giftChanceDecimalPlaces = config.Bind(
                GeneralSection,
                "GiftChanceDecimalPlaces",
                2,
                LocalizationIds.DescGiftChanceDecimalPlaces.GetLocalized(),
                range: new AcceptableValueRange<int>(0, 3),
                displayName: LocalizationIds.DisplayGiftChanceDecimalPlaces.GetLocalized(),
                sectionDisplayName: generalSectionDisplayName
            );

            _showBaseChance = config.Bind(
                GeneralSection,
                "ShowBaseChance",
                true,
                LocalizationIds.DescShowBaseChance.GetLocalized(),
                displayName: LocalizationIds.DisplayShowBaseChance.GetLocalized(),
                sectionDisplayName: generalSectionDisplayName
            );

            var bonusSectionDisplayName = LocalizationIds.SectionGiftChanceBonus.GetLocalized();

            _zayinBonus = BindBonusPercentage(
                config,
                "ZayinBonusPercentage",
                "ZAYIN",
                LocalizationIds.DisplayZayinBonus,
                bonusSectionDisplayName
            );
            _tethBonus = BindBonusPercentage(
                config,
                "TethBonusPercentage",
                "TETH",
                LocalizationIds.DisplayTethBonus,
                bonusSectionDisplayName
            );
            _heBonus = BindBonusPercentage(
                config,
                "HeBonusPercentage",
                "HE",
                LocalizationIds.DisplayHeBonus,
                bonusSectionDisplayName
            );
            _wawBonus = BindBonusPercentage(
                config,
                "WawBonusPercentage",
                "WAW",
                LocalizationIds.DisplayWawBonus,
                bonusSectionDisplayName
            );
            _alephBonus = BindBonusPercentage(
                config,
                "AlephBonusPercentage",
                "ALEPH",
                LocalizationIds.DisplayAlephBonus,
                bonusSectionDisplayName
            );
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
            string riskLevelName,
            string displayNameId,
            string sectionDisplayName
        )
        {
            var description = string.Format(
                CultureInfo.InvariantCulture,
                LocalizationIds.DescBonusPercentage.GetLocalized(),
                riskLevelName
            );

            return config.Bind(
                BonusSection,
                key,
                DefaultBonusPercentage,
                description,
                range: new AcceptableValueRange<float>(0.0f, 100.0f),
                displayName: displayNameId.GetLocalized(),
                useSlider: true,
                sectionDisplayName: sectionDisplayName
            );
        }
    }
}
