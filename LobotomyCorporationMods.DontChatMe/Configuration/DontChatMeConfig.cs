// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Configuration
{
    /// <summary>Configuration backed by Common's configuration abstractions.</summary>
    public sealed class DontChatMeConfig : IDontChatMeConfig
    {
        private const string ModId = "DontChatMe";
        private const string ModName = "Don't Chat Me";
        private const string ConnectionSection = "Connection";
        private const string EffectsSection = "Effects";
        private const string LimitsSection = "Limits";

        private readonly IConfigEntry<string> _serverUrl;
        private readonly IConfigEntry<string> _authToken;
        private readonly IConfigEntry<bool> _enabled;
        private readonly IConfigEntry<bool> _dangerEffectsEnabled;
        private readonly IConfigEntry<int> _maxInFlight;
        private readonly IConfigEntry<float> _globalCooldownSeconds;
        private readonly IConfigEntry<float> _energyAmount;
        private readonly IConfigEntry<int> _moneyAmount;

        public DontChatMeConfig()
        {
            var version = typeof(DontChatMeConfig).Assembly.GetName().Version.ToString(3);
            var config = new ModConfig(ModId, ModName, version);

            var connectionSectionName = LocalizationIds.SectionConnection.GetLocalized();
            var effectsSectionName = LocalizationIds.SectionEffects.GetLocalized();
            var limitsSectionName = LocalizationIds.SectionLimits.GetLocalized();

            _serverUrl = config.Bind(
                ConnectionSection,
                "ServerUrl",
                string.Empty,
                LocalizationIds.DescServerUrl.GetLocalized(),
                displayName: LocalizationIds.DisplayServerUrl.GetLocalized(),
                sectionDisplayName: connectionSectionName
            );

            _authToken = config.Bind(
                ConnectionSection,
                "AuthToken",
                string.Empty,
                LocalizationIds.DescAuthToken.GetLocalized(),
                displayName: LocalizationIds.DisplayAuthToken.GetLocalized(),
                sectionDisplayName: connectionSectionName
            );

            _enabled = config.Bind(
                ConnectionSection,
                "Enabled",
                defaultValue: true,
                LocalizationIds.DescEnabled.GetLocalized(),
                displayName: LocalizationIds.DisplayEnabled.GetLocalized(),
                sectionDisplayName: connectionSectionName
            );

            _dangerEffectsEnabled = config.Bind(
                EffectsSection,
                "DangerEffectsEnabled",
                defaultValue: false,
                LocalizationIds.DescDangerEffectsEnabled.GetLocalized(),
                displayName: LocalizationIds.DisplayDangerEffectsEnabled.GetLocalized(),
                sectionDisplayName: effectsSectionName
            );

            _energyAmount = config.Bind(
                EffectsSection,
                "EnergyAmount",
                defaultValue: 10.0f,
                LocalizationIds.DescEnergyAmount.GetLocalized(),
                range: new AcceptableValueRange<float>(1.0f, 500.0f),
                displayName: LocalizationIds.DisplayEnergyAmount.GetLocalized(),
                useSlider: true,
                sectionDisplayName: effectsSectionName
            );

            _moneyAmount = config.Bind(
                EffectsSection,
                "MoneyAmount",
                defaultValue: 100,
                LocalizationIds.DescMoneyAmount.GetLocalized(),
                range: new AcceptableValueRange<int>(1, 10_000),
                displayName: LocalizationIds.DisplayMoneyAmount.GetLocalized(),
                useSlider: true,
                sectionDisplayName: effectsSectionName
            );

            _maxInFlight = config.Bind(
                LimitsSection,
                "MaxInFlight",
                defaultValue: 32,
                LocalizationIds.DescMaxInFlight.GetLocalized(),
                range: new AcceptableValueRange<int>(1, 256),
                displayName: LocalizationIds.DisplayMaxInFlight.GetLocalized(),
                useSlider: true,
                sectionDisplayName: limitsSectionName
            );

            _globalCooldownSeconds = config.Bind(
                LimitsSection,
                "GlobalCooldownSeconds",
                defaultValue: 0.0f,
                LocalizationIds.DescGlobalCooldownSeconds.GetLocalized(),
                range: new AcceptableValueRange<float>(0.0f, 60.0f),
                displayName: LocalizationIds.DisplayGlobalCooldownSeconds.GetLocalized(),
                useSlider: true,
                sectionDisplayName: limitsSectionName
            );
        }

        public Uri ServerUrl
        {
            get
            {
                var raw = _serverUrl.Value;
                if (string.IsNullOrEmpty(raw))
                {
                    return null;
                }

                Uri parsed;
                return Uri.TryCreate(raw, UriKind.Absolute, out parsed) ? parsed : null;
            }
            set => _serverUrl.Value = value == null ? string.Empty : value.ToString();
        }

        public string AuthToken
        {
            get => _authToken.Value;
            set => _authToken.Value = value ?? string.Empty;
        }

        public bool Enabled
        {
            get => _enabled.Value;
            set => _enabled.Value = value;
        }

        public bool DangerEffectsEnabled => _dangerEffectsEnabled.Value;
        public int MaxInFlight => _maxInFlight.Value;
        public float GlobalCooldownSeconds => _globalCooldownSeconds.Value;
        public float EnergyAmount => _energyAmount.Value;
        public int MoneyAmount => _moneyAmount.Value;
    }
}
