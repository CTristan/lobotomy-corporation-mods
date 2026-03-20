// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using DebugPanel.Common.Implementations;
using DebugPanel.Interfaces;

#endregion

namespace DebugPanel.Implementations
{
    public sealed class FallbackPatchInspectionSource : IPatchInspectionSource
    {
        private readonly IPatchInspectionSource _primarySource;
        private readonly IPatchInspectionSource _fallbackSource;
        private readonly IList<string> _diagnosticLog;
        private readonly string _primaryLabel;
        private readonly string _fallbackLabel;

        public FallbackPatchInspectionSource(
            IPatchInspectionSource primarySource,
            IPatchInspectionSource fallbackSource,
            IList<string> diagnosticLog,
            string primaryLabel,
            string fallbackLabel)
        {
            ThrowHelper.ThrowIfNull(primarySource);
            _primarySource = primarySource;
            ThrowHelper.ThrowIfNull(fallbackSource);
            _fallbackSource = fallbackSource;
            ThrowHelper.ThrowIfNull(diagnosticLog);
            _diagnosticLog = diagnosticLog;
            ThrowHelper.ThrowIfNull(primaryLabel);
            _primaryLabel = primaryLabel;
            ThrowHelper.ThrowIfNull(fallbackLabel);
            _fallbackLabel = fallbackLabel;
        }

        public IEnumerable<PatchInspectionInfo> GetPatches()
        {
            List<PatchInspectionInfo> primaryResults;

            try
            {
                _diagnosticLog.Add("Active patch scan: trying " + _primaryLabel + "...");
                primaryResults = MaterializePatches(_primarySource.GetPatches());
                _diagnosticLog.Add(_primaryLabel + ": returned " + primaryResults.Count.ToString(CultureInfo.InvariantCulture) + " patches");
            }
            catch (Exception ex)
            {
                _diagnosticLog.Add(_primaryLabel + ": threw " + ex.GetType().Name + ": " + ex.Message);
                primaryResults = new List<PatchInspectionInfo>();
            }

            if (primaryResults.Count > 0)
            {
                return primaryResults;
            }

            _diagnosticLog.Add(_primaryLabel + " returned 0 patches, falling back to " + _fallbackLabel + "...");

            try
            {
                var fallbackResults = MaterializePatches(_fallbackSource.GetPatches());
                _diagnosticLog.Add(_fallbackLabel + ": returned " + fallbackResults.Count.ToString(CultureInfo.InvariantCulture) + " patches");

                return fallbackResults;
            }
            catch (Exception ex)
            {
                _diagnosticLog.Add(_fallbackLabel + ": threw " + ex.GetType().Name + ": " + ex.Message);

                return new List<PatchInspectionInfo>();
            }
        }

        private static List<PatchInspectionInfo> MaterializePatches(IEnumerable<PatchInspectionInfo> patches)
        {
            var list = new List<PatchInspectionInfo>();
            foreach (var patch in patches)
            {
                if (patch != null)
                {
                    list.Add(patch);
                }
            }

            return list;
        }
    }
}
