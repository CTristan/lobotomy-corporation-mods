// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class BepInExPluginCollector(IPluginInfoSource pluginInfoSource, IHarmonyVersionClassifier harmonyVersionClassifier) : IInfoCollector<IList<ModInfo>>
    {
        private readonly IPluginInfoSource _pluginInfoSource = pluginInfoSource ?? throw new ArgumentNullException(nameof(pluginInfoSource));
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier = harmonyVersionClassifier ?? throw new ArgumentNullException(nameof(harmonyVersionClassifier));

        public BepInExPluginCollector()
            : this(new ChainloaderPluginInfoSource(), new HarmonyVersionClassifier())
        {
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
