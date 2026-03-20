// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    /// <summary>
    ///     Standalone fallback that reads raw DLL bytes and scans for known Harmony reference name patterns.
    ///     Less precise than Cecil (could match string literals), but works without BepInEx.
    /// </summary>
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class BasicDllFileInspector : IDllFileInspector
    {
        private readonly PeAssemblyRefReader _peReader;

        public BasicDllFileInspector(PeAssemblyRefReader peReader)
        {
            _peReader = peReader;
        }

        public bool IsDeepInspectionAvailable => false;

        public IList<string> GetAssemblyReferences(string dllPath)
        {
            try
            {
                var bytes = File.ReadAllBytes(dllPath);

                return _peReader.ReadAssemblyReferences(bytes);
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
