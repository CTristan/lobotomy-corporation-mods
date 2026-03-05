// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class BaseModCollector : IInfoCollector<IList<ModInfo>>
    {
        private static readonly char[] s_pathSeparators = { '/' };

        private readonly ICollectorFileSystem _fileSystem;
        private readonly IBaseDirectoryProvider _baseDirectoryProvider;
        private readonly IAssemblyInspectionSource _assemblySource;
        private readonly IHarmonyVersionClassifier _harmonyVersionClassifier;

        public BaseModCollector()
            : this(
                new CollectorFileSystem(),
                new AppDomainBaseDirectoryProvider(),
                new AppDomainAssemblyInspectionSource(),
                new HarmonyVersionClassifier())
        {
        }

        public BaseModCollector(
            ICollectorFileSystem fileSystem,
            IBaseDirectoryProvider baseDirectoryProvider,
            IAssemblyInspectionSource assemblySource,
            IHarmonyVersionClassifier harmonyVersionClassifier)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _baseDirectoryProvider = baseDirectoryProvider ?? throw new ArgumentNullException(nameof(baseDirectoryProvider));
            _assemblySource = assemblySource ?? throw new ArgumentNullException(nameof(assemblySource));
            _harmonyVersionClassifier = harmonyVersionClassifier ?? throw new ArgumentNullException(nameof(harmonyVersionClassifier));
        }

        public IList<ModInfo> Collect()
        {
            var knownDirectories = GetKnownBaseModDirectories();
            var collected = new List<ModInfo>();
            var discoveredByAssembly = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var assembly in _assemblySource.GetAssemblies())
            {
                if (assembly == null || string.IsNullOrEmpty(assembly.Location))
                {
                    continue;
                }

                string modName;
                if (!TryGetBaseModNameFromPath(assembly.Location, out modName))
                {
                    continue;
                }

                discoveredByAssembly.Add(modName);
                collected.Add(new ModInfo(
                    modName,
                    assembly.Version,
                    ModSource.Lmm,
                    _harmonyVersionClassifier.Classify(assembly.References),
                    assembly.Name,
                    string.Empty));
            }

            foreach (var knownDirectory in knownDirectories)
            {
                if (!discoveredByAssembly.Contains(knownDirectory))
                {
                    collected.Add(new ModInfo(
                        knownDirectory,
                        "Unknown",
                        ModSource.Lmm,
                        HarmonyVersion.Unknown,
                        string.Empty,
                        string.Empty));
                }
            }

            return collected;
        }

        private List<string> GetKnownBaseModDirectories()
        {
            var baseDirectory = _baseDirectoryProvider.GetBaseDirectory();
            var roots = new List<string>
            {
                Path.Combine(Path.Combine(baseDirectory, "LobotomyCorp_Data"), "BaseMods"),
                Path.Combine(baseDirectory, "BaseMods"),
            };

            var knownDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var root in roots)
            {
                if (!_fileSystem.DirectoryExists(root))
                {
                    continue;
                }

                foreach (var directory in _fileSystem.EnumerateDirectories(root))
                {
                    var directoryName = Path.GetFileName(directory);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        knownDirectories.Add(directoryName);
                    }
                }
            }

            return new List<string>(knownDirectories);
        }

        public static bool TryGetBaseModNameFromPath(string path, out string modName)
        {
            modName = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var normalized = path.Replace('\\', '/');
            var segments = normalized.Split(s_pathSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (var index = 0; index < segments.Length - 1; index++)
            {
                if (!segments[index].Equals("BaseMods", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                modName = segments[index + 1];
                return !string.IsNullOrEmpty(modName);
            }

            return false;
        }
    }
}
