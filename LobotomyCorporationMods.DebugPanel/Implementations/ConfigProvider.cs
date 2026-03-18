// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Interfaces;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.DebugPanel.JsonModels;

#endregion

namespace Hemocode.DebugPanel.Implementations
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
