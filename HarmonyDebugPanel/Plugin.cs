// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using BepInEx;
using HarmonyDebugPanel.Formatting;
using HarmonyDebugPanel.Models;
using HarmonyDebugPanel.Rendering;
using UnityEngine;

namespace HarmonyDebugPanel
{
#pragma warning disable CA2243 // BepInEx uses reverse-domain IDs, not RFC GUID strings
    [BepInPlugin(PluginConstants.PluginGuid, PluginConstants.PluginName, PluginConstants.PluginVersion)]
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
#pragma warning restore CA2243
    [ExcludeFromCodeCoverage(Justification = "Unity/BepInEx runtime integration")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private PluginConfiguration _configuration;
        private DiagnosticReportBuilder _reportBuilder;
        private DiagnosticOverlay _diagnosticOverlay;
        private DiagnosticReport _report;
        private bool _isOverlayVisible;

        private void Awake()
        {
            _configuration = PluginConfiguration.Bind(Config);
            _reportBuilder = new DiagnosticReportBuilder();
            _diagnosticOverlay = new DiagnosticOverlay();

            RefreshReport();

            Logger.LogInfo("HarmonyDebugPanel initialized. Press " + _configuration.OverlayToggleHotkey.Value + " to toggle the overlay.");
            Logger.LogInfo("Press " + _configuration.RefreshHotkey.Value + " to refresh diagnostic information.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(_configuration.OverlayToggleHotkey.Value))
            {
                _isOverlayVisible = !_isOverlayVisible;
            }

            if (Input.GetKeyDown(_configuration.RefreshHotkey.Value))
            {
                RefreshReport();
            }
        }

        private void OnGUI()
        {
            if (!_isOverlayVisible)
            {
                return;
            }

            _diagnosticOverlay.Draw(_report, _configuration, RefreshReport);
        }

        private void RefreshReport()
        {
            _report = _reportBuilder.BuildReport();
            var lines = DiagnosticLogFormatter.Format(_report);
            foreach (var line in lines)
            {
                Logger.LogInfo(line);
            }
        }
    }
}
