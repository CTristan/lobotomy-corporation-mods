// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
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
        private const string LogFolderName = "logs";
        private const string LogFileName = "HarmonyDebugPanel.log";

        private string _logFilePath;

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
                // Logger may not be initialized yet - use Debug.Log as fallback
                if (Logger != null)
                {
                    Logger.LogInfo("HarmonyDebugPanel: Awake starting...");
                }
                else
                {
                    Debug.Log("HarmonyDebugPanel: Awake starting...");
                }

                if (Config == null)
                {
                    if (Logger != null)
                    {
                        Logger.LogWarning("HarmonyDebugPanel: BepInEx Config is null (constructor bug?). Using defaults.");
                    }
                    else
                    {
                        Debug.LogWarning("HarmonyDebugPanel: BepInEx Config is null (constructor bug?). Using defaults.");
                    }
                }

                _startTime = Time.time;
                _configuration = PluginConfiguration.Bind(Config);
                _reportBuilder = new DiagnosticReportBuilder();
                _diagnosticOverlay = new DiagnosticOverlay();

                // Initialize log file path
                InitializeLogFile();

                // Build initial report immediately
                _report = _reportBuilder.BuildReport();

                if (Logger != null)
                {
                    Logger.LogInfo("HarmonyDebugPanel fully initialized.");
                }
                else
                {
                    Debug.Log("HarmonyDebugPanel fully initialized.");
                }
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger.LogError("HarmonyDebugPanel initialization error: " + ex);
                }
                else
                {
                    Debug.LogError("HarmonyDebugPanel initialization error: " + ex);
                }
            }
        }
#pragma warning restore CA1031

#pragma warning disable CA1031
        private void InitializeLogFile()
        {
            try
            {
                // Use Assembly.GetExecutingAssembly().Location instead of Info.Location
                // which may not be available during early startup in BaseMods context
                var location = Assembly.GetExecutingAssembly().Location;

                // Find the plugin directory - handle both Unix and Windows path separators
                string pluginDirectory;
                var lastSep = Math.Max(location.LastIndexOf('/'), location.LastIndexOf('\\'));
                if (lastSep > 0)
                {
                    pluginDirectory = location.Substring(0, lastSep);
                }
                else
                {
                    pluginDirectory = Environment.CurrentDirectory;
                }

                var logDirectory = Path.Combine(pluginDirectory, LogFolderName);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                _logFilePath = Path.Combine(logDirectory, LogFileName);

                // Write initial message to log file
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                File.WriteAllText(_logFilePath, $"[{timestamp}] INFO: HarmonyDebugPanel log file initialized{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                if (Logger != null)
                {
                    Logger.LogWarning("HarmonyDebugPanel: Could not initialize log file: " + ex.Message);
                }
                _logFilePath = null;
            }
        }
#pragma warning restore CA1031

        private void LogInfo(string message)
        {
            WriteToLogFile("INFO", message);
            if (Logger != null)
            {
                Logger.LogInfo(message);
            }
        }

        private void LogWarning(string message)
        {
            WriteToLogFile("WARN", message);
            if (Logger != null)
            {
                Logger.LogWarning(message);
            }
        }

        private void LogError(string message)
        {
            WriteToLogFile("ERROR", message);
            if (Logger != null)
            {
                Logger.LogError(message);
            }
        }

        private void WriteToLogFile(string level, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
            {
                return;
            }

            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                var logLine = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logLine, Encoding.UTF8);
            }
            catch (IOException)
            {
                // Silently ignore file write errors to prevent cascading failures
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
            catch
            {
                // Silently ignore Update errors to prevent cascading failures
            }
        }

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
    }
}
