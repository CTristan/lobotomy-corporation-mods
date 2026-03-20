// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Interfaces;
using DebugPanel.Interfaces;
using DebugPanel.JsonModels;

#endregion

namespace DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class ConfigProvider : IConfigProvider
    {
        private const string ConfigFileName = "DebugPanel.config.json";

        private readonly IFileManager _fileManager;

        public ConfigProvider(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public DebugPanelConfig LoadConfig()
        {
            var json = _fileManager.ReadAllText(ConfigFileName, false);

            if (string.IsNullOrEmpty(json))
            {
                return new DebugPanelConfig();
            }

            var config = UnityEngine.JsonUtility.FromJson<DebugPanelConfig>(json);

            return config ?? new DebugPanelConfig();
        }

        public void SaveConfig(DebugPanelConfig config)
        {
            var json = UnityEngine.JsonUtility.ToJson(config, true);
            _fileManager.WriteAllText(ConfigFileName, json);
        }
    }
}
