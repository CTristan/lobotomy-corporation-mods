// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.DebugPanel.Interfaces;

#endregion

namespace Hemocode.DebugPanel.Implementations
{
    /// <summary>
    ///     Reflects into Mono.Cecil's AssemblyDefinition to extract precise assembly references from DLL files on disk.
    ///     Only instantiated when Mono.Cecil is available at runtime (BepInEx installed).
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class CecilDllFileInspector : IDllFileInspector
    {
        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public bool IsDeepInspectionAvailable => true;

        public IList<string> GetAssemblyReferences(string dllPath)
        {
            var references = new List<string>();

            if (string.IsNullOrEmpty(dllPath))
            {
                return references;
            }

            object assemblyDefinition = null;

            try
            {
                var assemblyDefinitionType = FindCecilType("Mono.Cecil.AssemblyDefinition");
                var readerParametersType = FindCecilType("Mono.Cecil.ReaderParameters");

                if (assemblyDefinitionType == null || readerParametersType == null)
                {
                    return references;
                }

                var readerParams = Activator.CreateInstance(readerParametersType);
                var readWriteProperty = readerParametersType.GetProperty("ReadWrite", PublicInstance);
                readWriteProperty?.SetValue(readerParams, false, null);

                var readAssemblyMethod = assemblyDefinitionType.GetMethod(
                    "ReadAssembly",
                    PublicStatic,
                    null,
                    new[] { typeof(string), readerParametersType },
                    null);

                if (readAssemblyMethod == null)
                {
                    return references;
                }

                assemblyDefinition = readAssemblyMethod.Invoke(null, new[] { dllPath, readerParams });

                if (assemblyDefinition == null)
                {
                    return references;
                }

                var mainModuleProperty = assemblyDefinitionType.GetProperty("MainModule", PublicInstance);
                if (mainModuleProperty == null)
                {
                    return references;
                }

                var mainModule = mainModuleProperty.GetValue(assemblyDefinition, null);
                if (mainModule == null)
                {
                    return references;
                }

                var assemblyReferencesProperty = mainModule.GetType().GetProperty("AssemblyReferences", PublicInstance);
                if (assemblyReferencesProperty == null)
                {
                    return references;
                }

                if (!(assemblyReferencesProperty.GetValue(mainModule, null) is IEnumerable assemblyReferences))
                {
                    return references;
                }

                foreach (var reference in assemblyReferences)
                {
                    if (reference == null)
                    {
                        continue;
                    }

                    var nameProperty = reference.GetType().GetProperty("Name", PublicInstance);
                    if (nameProperty == null)
                    {
                        continue;
                    }

                    var name = nameProperty.GetValue(reference, null) as string;
                    if (!string.IsNullOrEmpty(name))
                    {
                        references.Add(name);
                    }
                }
            }
            catch (BadImageFormatException)
            {
                // Not a valid .NET assembly
            }
            catch (IOException)
            {
                // File access error
            }
            catch (InvalidOperationException)
            {
                // Cecil internal error
            }
            catch (Exception)
            {
                // Unexpected reflection or Cecil failure
            }
            finally
            {
                if (assemblyDefinition is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            return references;
        }

        private static Type FindCecilType(string typeName)
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
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception)
                {
                    // Skip assemblies that fail type resolution
                }
            }

            return null;
        }
    }
}
