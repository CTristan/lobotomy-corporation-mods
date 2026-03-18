// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Hemocode.Common.Implementations;
using Hemocode.DebugPanel.Interfaces;
using Hemocode.Common.Enums.Diagnostics;
using Hemocode.Common.Models.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    public sealed class BepInExPluginCollector : IInfoCollector<IList<DetectedModInfo>>
    {
        private readonly IPluginInfoSource _pluginInfoSource;
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier;

        public BepInExPluginCollector(IPluginInfoSource pluginInfoSource, IHarmonyVersionClassifier harmonyVersionClassifier)
        {
            ThrowHelper.ThrowIfNull(pluginInfoSource);
            _pluginInfoSource = pluginInfoSource;
            ThrowHelper.ThrowIfNull(harmonyVersionClassifier);
            _harmonyVersionClassifier = harmonyVersionClassifier;
        }

        public IList<DetectedModInfo> Collect()
        {
            var plugins = _pluginInfoSource.GetPlugins();
            var results = new List<DetectedModInfo>();

            foreach (var plugin in plugins)
            {
                if (plugin == null)
                {
                    continue;
                }

                var harmonyVersion = _harmonyVersionClassifier.Classify(plugin.Assembly.References);
                results.Add(new DetectedModInfo(
                    plugin.Name,
                    plugin.Version,
                    ModSource.BepInExPlugin,
                    harmonyVersion,
                    plugin.Assembly.Name,
                    plugin.PluginId,
                    false,
                    0,
                    0));
            }

            return results;
        }
    }
}
