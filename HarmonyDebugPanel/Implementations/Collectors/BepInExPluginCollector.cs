// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class BepInExPluginCollector : IInfoCollector<IList<ModInfo>>
    {
        private readonly IPluginInfoSource _pluginInfoSource;
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier;

        public BepInExPluginCollector()
            : this(new ChainloaderPluginInfoSource(), new HarmonyVersionClassifier())
        {
        }

        public BepInExPluginCollector(IPluginInfoSource pluginInfoSource, IHarmonyVersionClassifier harmonyVersionClassifier)
        {
            _pluginInfoSource = pluginInfoSource ?? throw new ArgumentNullException(nameof(pluginInfoSource));
            _harmonyVersionClassifier = harmonyVersionClassifier ?? throw new ArgumentNullException(nameof(harmonyVersionClassifier));
        }

        public IList<ModInfo> Collect()
        {
            var plugins = _pluginInfoSource.GetPlugins();
            var results = new List<ModInfo>();

            foreach (var plugin in plugins)
            {
                if (plugin == null)
                {
                    continue;
                }

                var harmonyVersion = _harmonyVersionClassifier.Classify(plugin.Assembly.References);
                results.Add(new ModInfo(
                    plugin.Name,
                    plugin.Version,
                    ModSource.BepInExPlugin,
                    harmonyVersion,
                    plugin.Assembly.Name,
                    plugin.PluginId));
            }

            return results;
        }
    }
}
