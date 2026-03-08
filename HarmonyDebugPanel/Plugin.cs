// SPDX-License-Identifier: MIT

using System;
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
#pragma warning restore CA2243
    [ExcludeFromCodeCoverage(Justification = "Unity/BepInEx runtime integration")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const int AutoRefreshFrameInterval = 60;
        private const int AutoRefreshDurationSeconds = 60;

        private PluginConfiguration _configuration;
        private DiagnosticReportBuilder _reportBuilder;
        private DiagnosticOverlay _diagnosticOverlay;
        private DiagnosticReport _report;
        private bool _isOverlayVisible = true;

        private float _startTime;
        private int _frameCount;

#pragma warning disable CA1031
        private void Awake()
        {
            try
            {
                if (Config == null)
                {
                    LogWarning("HarmonyDebugPanel: BepInEx Config is null (constructor bug?). Using defaults.");
                }

                _startTime = Time.time;
                _configuration = PluginConfiguration.Bind(Config);
                _reportBuilder = new DiagnosticReportBuilder();
                _diagnosticOverlay = new DiagnosticOverlay();

                // Build initial report immediately
                _report = _reportBuilder.BuildReport();

                LogInfo("HarmonyDebugPanel fully initialized.");
            }
            catch (Exception ex)
            {
                LogError("HarmonyDebugPanel initialization error: " + ex);
            }
        }
#pragma warning restore CA1031

        private void LogInfo(string message)
        {
            if (Logger != null)
            {
                Logger.LogInfo(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (Logger != null)
            {
                Logger.LogWarning(message);
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private void LogError(string message)
        {
            if (Logger != null)
            {
                Logger.LogError(message);
            }
            else
            {
                Debug.LogError(message);
            }
        }

#pragma warning disable CA1031
        private void Update()
        {
            try
            {
                if (_configuration == null)
                {
                    return;
                }

                if (Input.GetKeyDown(_configuration.OverlayToggleHotkey))
                {
                    _isOverlayVisible = !_isOverlayVisible;
                }

                if (Input.GetKeyDown(_configuration.RefreshHotkey))
                {
                    RefreshReport();
                }

                if (!_isOverlayVisible || _reportBuilder == null)
                {
                    return;
                }

                _frameCount++;

                var elapsedTime = Time.time - _startTime;

                if (elapsedTime < AutoRefreshDurationSeconds && _frameCount % AutoRefreshFrameInterval == 0)
                {
                    RefreshReport();
                }
            }
            catch (Exception)
            {
                // Silently ignore Update errors to prevent cascading failures
            }
        }

#pragma warning disable CA1031
        private void OnGUI()
        {
            // ALWAYS draw this test box first - no conditions
            GUI.Box(new Rect(10, 10, 300, 30), "HarmonyDebugPanel running!");

            if (!_isOverlayVisible)
            {
                return;
            }

            if (_report == null)
            {
                GUI.Box(new Rect(10, 50, 300, 30), "Report is NULL");
                return;
            }

            if (_configuration == null)
            {
                GUI.Box(new Rect(10, 50, 300, 30), "Config is NULL");
                return;
            }

            try
            {
                _diagnosticOverlay.Draw(_report, _configuration, RefreshReport);
            }
            catch (Exception ex)
            {
                GUI.Box(new Rect(10, 50, 400, 30), "Draw error: " + ex.Message);
            }
        }
#pragma warning restore CA1031

#pragma warning disable CA1031
        private void RefreshReport()
        {
            if (_reportBuilder == null)
            {
                return;
            }

            try
            {
                _report = _reportBuilder.BuildReport();
            }
            catch (Exception ex)
            {
                LogError("RefreshReport error: " + ex);
            }
        }
#pragma warning restore CA1031
    }
}
