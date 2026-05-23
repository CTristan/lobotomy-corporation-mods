// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private readonly IConfigEntry<int> _gameId;
        private readonly IConfigEntry<bool> _enabled;
        private readonly IConfigEntry<bool> _dangerEffectsEnabled;
        private readonly IConfigEntry<float> _globalCooldownSeconds;
        private readonly IConfigEntry<float> _energyAmount;
        private readonly IConfigEntry<int> _moneyAmount;
        private readonly string _configFilePath;

        public DontChatMeConfig(string configFilePath = null)
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

            _gameId = config.Bind(
                ConnectionSection,
                "GameId",
                defaultValue: 0,
                LocalizationIds.DescGameId.GetLocalized(),
                displayName: LocalizationIds.DisplayGameId.GetLocalized(),
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

            if (configFilePath != null)
            {
                _configFilePath = configFilePath;
                LoadFromFile(configFilePath);
            }
        }

        public void Save()
        {
            if (_configFilePath == null)
            {
                return;
            }

            try
            {
                var dir = Path.GetDirectoryName(_configFilePath);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                }

                var lines = File.Exists(_configFilePath)
                    ? new List<string>(File.ReadAllLines(_configFilePath))
                    : new List<string>();

                SetIniValue(lines, ConnectionSection, "ServerUrl", _serverUrl.Value);
                SetIniValue(lines, ConnectionSection, "AuthToken", _authToken.Value);
                SetIniValue(
                    lines,
                    ConnectionSection,
                    "GameId",
                    _gameId.Value.ToString(CultureInfo.InvariantCulture)
                );
                SetIniValue(lines, ConnectionSection, "Enabled", _enabled.Value ? "True" : "False");

                File.WriteAllLines(_configFilePath, lines.ToArray());
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var section = string.Empty;
                foreach (var line in File.ReadAllLines(filePath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (
                        trimmed.StartsWith("[", StringComparison.Ordinal)
                        && trimmed.EndsWith("]", StringComparison.Ordinal)
                    )
                    {
                        section = trimmed.Substring(1, trimmed.Length - 2).Trim();
                        continue;
                    }

                    if (
                        !string.Equals(
                            section,
                            ConnectionSection,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }

                    var eq = trimmed.IndexOf('=');
                    if (eq < 0)
                    {
                        continue;
                    }

                    var key = trimmed.Substring(0, eq).Trim();
                    var value = trimmed.Substring(eq + 1).Trim();

                    if (string.Equals(key, "ServerUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        _serverUrl.Value = value;
                    }
                    else if (string.Equals(key, "AuthToken", StringComparison.OrdinalIgnoreCase))
                    {
                        _authToken.Value = value;
                    }
                    else if (string.Equals(key, "GameId", StringComparison.OrdinalIgnoreCase))
                    {
                        int parsedGameId;
                        if (
                            int.TryParse(
                                value,
                                NumberStyles.Integer,
                                CultureInfo.InvariantCulture,
                                out parsedGameId
                            )
                        )
                        {
                            _gameId.Value = parsedGameId;
                        }
                    }
                    else if (string.Equals(key, "Enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        bool b;
                        if (bool.TryParse(value, out b))
                        {
                            _enabled.Value = b;
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private static void SetIniValue(
            List<string> lines,
            string section,
            string key,
            string value
        )
        {
            var sectionHeader = "[" + section + "]";
            var inSection = false;

            for (var i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();

                if (string.Equals(trimmed, sectionHeader, StringComparison.OrdinalIgnoreCase))
                {
                    inSection = true;
                    continue;
                }

                if (trimmed.StartsWith("[", StringComparison.Ordinal))
                {
                    if (inSection)
                    {
                        // Key was absent from our section; insert before the next section header.
                        lines.Insert(i, key + " = " + value);
                        return;
                    }

                    inSection = false;
                    continue;
                }

                if (
                    !inSection
                    || trimmed.StartsWith("#", StringComparison.Ordinal)
                    || trimmed.Length == 0
                )
                {
                    continue;
                }

                var eq = trimmed.IndexOf('=');
                if (eq < 0)
                {
                    continue;
                }

                if (
                    string.Equals(
                        trimmed.Substring(0, eq).Trim(),
                        key,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    lines[i] = key + " = " + value;
                    return;
                }
            }

            // Section not found, or key missing from section at end-of-file — append.
            if (!inSection)
            {
                if (lines.Count > 0 && lines[lines.Count - 1].Trim().Length > 0)
                {
                    lines.Add(string.Empty);
                }

                lines.Add(sectionHeader);
            }

            lines.Add(key + " = " + value);
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

        public int GameId
        {
            get => _gameId.Value;
            set => _gameId.Value = value;
        }

        public bool Enabled
        {
            get => _enabled.Value;
            set => _enabled.Value = value;
        }

        public bool DangerEffectsEnabled => _dangerEffectsEnabled.Value;
        public float GlobalCooldownSeconds => _globalCooldownSeconds.Value;
        public float EnergyAmount => _energyAmount.Value;
        public int MoneyAmount => _moneyAmount.Value;
    }
}
