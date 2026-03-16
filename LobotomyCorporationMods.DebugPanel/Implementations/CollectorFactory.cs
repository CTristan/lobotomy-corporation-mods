// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using LobotomyCorporationMods.Common.Models.Diagnostics;

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
            ThrowHelper.ThrowIfNull(environmentDetector);
            _environmentDetector = environmentDetector;
            ThrowHelper.ThrowIfNull(harmony1Source);
            _harmony1Source = harmony1Source;
            ThrowHelper.ThrowIfNull(harmony2Source);
            _harmony2Source = harmony2Source;
            ThrowHelper.ThrowIfNull(assemblySource);
            _assemblySource = assemblySource;
            ThrowHelper.ThrowIfNull(pluginInfoSource);
            _pluginInfoSource = pluginInfoSource;
            ThrowHelper.ThrowIfNull(classifier);
            _classifier = classifier;
            ThrowHelper.ThrowIfNull(basicDllInspector);
            _basicDllInspector = basicDllInspector;
            ThrowHelper.ThrowIfNull(cecilDllInspector);
            _cecilDllInspector = cecilDllInspector;
            ThrowHelper.ThrowIfNull(shimArtifactSource);
            _shimArtifactSource = shimArtifactSource;
            ThrowHelper.ThrowIfNull(loadedAssemblySource);
            _loadedAssemblySource = loadedAssemblySource;
        }

        public IActivePatchCollector CreateActivePatchCollector(IList<string> debugInfo)
        {
            return new ActivePatchCollector(CreatePatchSource(debugInfo));
        }

        public IInfoCollector<IList<DetectedModInfo>> CreateBaseModCollector(IList<string> debugInfo)
        {
            return new BaseModCollector(CreatePatchSource(debugInfo), _classifier);
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

        public IExpectedPatchSource CreateExpectedPatchSource(IList<string> debugInfo)
        {
            return new ExpectedPatchSource(CreatePatchSource(debugInfo));
        }

        private IPatchInspectionSource CreatePatchSource(IList<string> debugInfo)
        {
            if (_environmentDetector.IsHarmony2Available)
            {
                return new FallbackPatchInspectionSource(_harmony2Source, _harmony1Source, debugInfo, "Harmony2", "Harmony1");
            }

            return _harmony1Source;
        }

        public IInfoCollector<DllIntegrityReport> CreateDllIntegrityCollector()
        {
            var inspector = _environmentDetector.IsMonoCecilAvailable ? _cecilDllInspector : _basicDllInspector;

            return new DllIntegrityCollector(inspector, _shimArtifactSource, _loadedAssemblySource);
        }
    }
}
