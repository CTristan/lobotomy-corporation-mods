// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace Hemocode.DebugPanel.JsonModels
{
    [Serializable]
    public class DebugPanelConfig
    {
        public string overlayToggleKey = "F9";
        public string refreshKey = "F10";
        public bool showActivePatches = true;
        public bool showExpectedPatches = true;
        public bool showAssemblyInfo = true;
        public bool showBepInExPlugins = true;
        public bool showLmmMods = true;
        public bool showDllIntegrity = true;
        public bool showFilesystemValidation = true;
        public bool showErrorLogs = true;
        public bool showKnownIssues = true;
        public bool showDependencies = true;

        public string OverlayToggleKey => overlayToggleKey;

        public string RefreshKey => refreshKey;

        public bool ShowActivePatches => showActivePatches;

        public bool ShowExpectedPatches => showExpectedPatches;

        public bool ShowAssemblyInfo => showAssemblyInfo;

        public bool ShowBepInExPlugins => showBepInExPlugins;

        public bool ShowLmmMods => showLmmMods;

        public bool ShowDllIntegrity => showDllIntegrity;

        public bool ShowFilesystemValidation => showFilesystemValidation;

        public bool ShowErrorLogs => showErrorLogs;

        public bool ShowKnownIssues => showKnownIssues;

        public bool ShowDependencies => showDependencies;
    }
}
