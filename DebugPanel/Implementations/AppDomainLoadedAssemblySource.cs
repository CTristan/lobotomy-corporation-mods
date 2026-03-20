// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    /// <summary>
    ///     Filters AppDomain assemblies to those loaded from the BaseMods directory,
    ///     extracting their referenced assembly names for integrity comparison.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class AppDomainLoadedAssemblySource : ILoadedAssemblyReferenceSource
    {
        public IList<LoadedAssemblyInfo> GetBaseModAssemblies()
        {
            var results = new List<LoadedAssemblyInfo>();

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    try
                    {
                        var location = assembly.Location;

                        if (string.IsNullOrEmpty(location))
                        {
                            continue;
                        }

                        if (location.IndexOf("BaseMods", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        var assemblyName = assembly.GetName();
                        var referencedAssemblies = assembly.GetReferencedAssemblies();
                        var refNames = new List<string>(referencedAssemblies.Length);

                        foreach (var reference in referencedAssemblies)
                        {
                            if (reference != null && reference.Name != null)
                            {
                                refNames.Add(reference.Name);
                            }
                        }

                        results.Add(new LoadedAssemblyInfo(
                            assemblyName.Name ?? "Unknown",
                            location,
                            refNames));
                    }
                    catch (NotSupportedException)
                    {
                        // Dynamic assemblies don't have a Location
                    }
                    catch (Exception)
                    {
                        // Skip individual assembly inspection failures
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list if AppDomain scanning fails entirely
            }

            return results;
        }
    }
}
