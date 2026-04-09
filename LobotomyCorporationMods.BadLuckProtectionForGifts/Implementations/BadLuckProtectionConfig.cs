// SPDX-License-Identifier: MIT

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
        private const string BonusSection = "Bonus Percentages";
        private const float DefaultBonusPercentage = 1.0f;

        private readonly LmmConfigEntry<bool> _normalizedBonusEnabled;
        private readonly LmmConfigEntry<bool> _resetOnGiftReceived;
        private readonly LmmConfigEntry<float> _zayinBonus;
        private readonly LmmConfigEntry<float> _tethBonus;
        private readonly LmmConfigEntry<float> _heBonus;
        private readonly LmmConfigEntry<float> _wawBonus;
        private readonly LmmConfigEntry<float> _alephBonus;

        public BadLuckProtectionConfig()
        {
            var configFile = LmmConfigRegistration.GetConfigFile(ModId, ModName);

            _normalizedBonusEnabled = configFile.Bind(
                GeneralSection,
                "NormalizedBonusEnabled",
                true,
                new LmmConfigDescription(
                    "Normalize gift chance increase across all abnormalities based on PE box ratio."
                )
            );

            _resetOnGiftReceived = configFile.Bind(
                GeneralSection,
                "ResetOnGiftReceived",
                false,
                new LmmConfigDescription(
                    "Reset an agent's work count for a gift when they receive that gift."
                )
            );

            _zayinBonus = BindBonusPercentage(configFile, "ZayinBonusPercentage", "ZAYIN");
            _tethBonus = BindBonusPercentage(configFile, "TethBonusPercentage", "TETH");
            _heBonus = BindBonusPercentage(configFile, "HeBonusPercentage", "HE");
            _wawBonus = BindBonusPercentage(configFile, "WawBonusPercentage", "WAW");
            _alephBonus = BindBonusPercentage(configFile, "AlephBonusPercentage", "ALEPH");
        }

        public bool NormalizedBonusEnabled => _normalizedBonusEnabled.Value;

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
            string riskLevelName
        )
        {
            return configFile.Bind(
                BonusSection,
                key,
                DefaultBonusPercentage,
                new LmmConfigDescription(
                    "Bonus percentage per successful work for " + riskLevelName + " abnormalities.",
                    new AcceptableValueRange<float>(0.0f, 10.0f)
                )
            );
        }
    }
}
