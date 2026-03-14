// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    public sealed class CollectorFactory : ICollectorFactory
    {
        private readonly IEnvironmentDetector _environmentDetector;
        private readonly IPatchInspectionSource _harmony1Source;
        private readonly IPatchInspectionSource _harmony2Source;
        private readonly IAssemblyInspectionSource _assemblySource;
        private readonly IPluginInfoSource _pluginInfoSource;
        private readonly IHarmonyVersionClassifier _classifier;

        public CollectorFactory(
            IEnvironmentDetector environmentDetector,
            IPatchInspectionSource harmony1Source,
            IPatchInspectionSource harmony2Source,
            IAssemblyInspectionSource assemblySource,
            IPluginInfoSource pluginInfoSource,
            IHarmonyVersionClassifier classifier)
        {
            _environmentDetector = Guard.Against.Null(environmentDetector, nameof(environmentDetector));
            _harmony1Source = Guard.Against.Null(harmony1Source, nameof(harmony1Source));
            _harmony2Source = Guard.Against.Null(harmony2Source, nameof(harmony2Source));
            _assemblySource = Guard.Against.Null(assemblySource, nameof(assemblySource));
            _pluginInfoSource = Guard.Against.Null(pluginInfoSource, nameof(pluginInfoSource));
            _classifier = Guard.Against.Null(classifier, nameof(classifier));
        }

        public IActivePatchCollector CreateActivePatchCollector()
        {
            var source = _environmentDetector.IsHarmony2Available ? _harmony2Source : _harmony1Source;

            return new ActivePatchCollector(source);
        }

        public IInfoCollector<IList<DetectedModInfo>> CreateBaseModCollector()
        {
            var source = _environmentDetector.IsHarmony2Available ? _harmony2Source : _harmony1Source;

            return new BaseModCollector(source, _classifier);
        }

        public IInfoCollector<IList<DetectedModInfo>> CreateBepInExPluginCollector()
        {
            return new BepInExPluginCollector(_pluginInfoSource, _classifier);
        }

        public IInfoCollector<IList<AssemblyInfo>> CreateAssemblyInfoCollector()
        {
            return new AssemblyInfoCollector(_assemblySource);
        }

        public IInfoCollector<RetargetHarmonyStatus> CreateRetargetHarmonyDetector()
        {
            return new RetargetHarmonyDetector(_assemblySource);
        }

        public IExpectedPatchSource CreateExpectedPatchSource()
        {
            var source = _environmentDetector.IsHarmony2Available ? _harmony2Source : _harmony1Source;

            return new ExpectedPatchSource(source);
        }
    }
}
