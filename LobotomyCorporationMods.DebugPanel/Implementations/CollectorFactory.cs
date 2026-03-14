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
        private readonly IDllFileInspector _basicDllInspector;
        private readonly IDllFileInspector _cecilDllInspector;
        private readonly IShimArtifactSource _shimArtifactSource;
        private readonly ILoadedAssemblyReferenceSource _loadedAssemblySource;

        public CollectorFactory(
            IEnvironmentDetector environmentDetector,
            IPatchInspectionSource harmony1Source,
            IPatchInspectionSource harmony2Source,
            IAssemblyInspectionSource assemblySource,
            IPluginInfoSource pluginInfoSource,
            IHarmonyVersionClassifier classifier,
            IDllFileInspector basicDllInspector,
            IDllFileInspector cecilDllInspector,
            IShimArtifactSource shimArtifactSource,
            ILoadedAssemblyReferenceSource loadedAssemblySource)
        {
            _environmentDetector = Guard.Against.Null(environmentDetector, nameof(environmentDetector));
            _harmony1Source = Guard.Against.Null(harmony1Source, nameof(harmony1Source));
            _harmony2Source = Guard.Against.Null(harmony2Source, nameof(harmony2Source));
            _assemblySource = Guard.Against.Null(assemblySource, nameof(assemblySource));
            _pluginInfoSource = Guard.Against.Null(pluginInfoSource, nameof(pluginInfoSource));
            _classifier = Guard.Against.Null(classifier, nameof(classifier));
            _basicDllInspector = Guard.Against.Null(basicDllInspector, nameof(basicDllInspector));
            _cecilDllInspector = Guard.Against.Null(cecilDllInspector, nameof(cecilDllInspector));
            _shimArtifactSource = Guard.Against.Null(shimArtifactSource, nameof(shimArtifactSource));
            _loadedAssemblySource = Guard.Against.Null(loadedAssemblySource, nameof(loadedAssemblySource));
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

        public IInfoCollector<DllIntegrityReport> CreateDllIntegrityCollector()
        {
            var inspector = _environmentDetector.IsMonoCecilAvailable ? _cecilDllInspector : _basicDllInspector;

            return new DllIntegrityCollector(inspector, _shimArtifactSource, _loadedAssemblySource);
        }
    }
}
