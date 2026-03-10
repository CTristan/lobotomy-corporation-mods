// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyDebugPanel.Formatting;
using HarmonyDebugPanel.Models;
using HarmonyDebugPanel.Rendering;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Process = System.Diagnostics.Process;

namespace HarmonyDebugPanel
{
    [BepInPlugin(PluginConstants.PluginGuid, PluginConstants.PluginName, PluginConstants.PluginVersion)]
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
                _diagnosticOverlay.Draw(_report, _configuration, RefreshReport, GenerateLog);
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

        private void GenerateLog()
        {
            try
            {
                LogInfo("GenerateLog: Starting log generation");
                LogInfo("GenerateLog: _logFilePath = " + _logFilePath);

                if (_report == null)
                {
                    LogError("GenerateLog: Report is null");
                    return;
                }

                // Build timestamped filename
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture);
                var logFileName = "HarmonyDebugPanel_" + timestamp + ".log";
                var logFilePath = Path.Combine(Path.GetDirectoryName(_logFilePath) ?? Environment.CurrentDirectory, logFileName);

                // Read RetargetHarmony log content (this also writes debug messages to runtime log)
                var retargetHarmonyLogContent = string.Empty;
                var retargetHarmonyLogPath = GetRetargetHarmonyLogPath();
                if (!string.IsNullOrEmpty(retargetHarmonyLogPath) && File.Exists(retargetHarmonyLogPath))
                {
                    try
                    {
                        retargetHarmonyLogContent = File.ReadAllText(retargetHarmonyLogPath, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        LogWarning("GenerateLog: Could not read RetargetHarmony log file: " + ex.Message);
                    }
                }

                // Read BepInEx LogOutput.log content (using snapshot reader to handle locked files)
                string bepInExLogContent = null; // null means file not found
                var bepInExLogPath = GetBepInExLogPath();
                if (!string.IsNullOrEmpty(bepInExLogPath) && File.Exists(bepInExLogPath))
                {
                    bepInExLogContent = ReadLogFileSnapshot(bepInExLogPath);
                    if (string.IsNullOrEmpty(bepInExLogContent))
                    {
                        bepInExLogContent = "[BepInEx LogOutput.log exists but could not be read - file is likely locked by another process]";
                    }
                }

                // Read runtime log content AFTER helper methods so we capture their debug messages
                var runtimeLogContent = string.Empty;
                if (!string.IsNullOrEmpty(_logFilePath) && File.Exists(_logFilePath))
                {
                    try
                    {
                        runtimeLogContent = File.ReadAllText(_logFilePath, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        LogWarning("GenerateLog: Could not read runtime log file: " + ex.Message);
                    }
                }

                // Read Unity output_log.txt content (using snapshot reader to handle locked files)
                string unityLogContent = null; // null means file not found
                var unityLogPath = GetUnityLogPath();
                if (!string.IsNullOrEmpty(unityLogPath) && File.Exists(unityLogPath))
                {
                    unityLogContent = ReadLogFileSnapshot(unityLogPath);
                    if (string.IsNullOrEmpty(unityLogContent))
                    {
                        unityLogContent = "[Unity output_log.txt exists but could not be read - file is likely locked by another process]";
                    }
                }

                // Format the extended log
                var logLines = DiagnosticLogFormatter.FormatExtended(_report, runtimeLogContent, retargetHarmonyLogContent, bepInExLogContent, unityLogContent);
                var logContent = string.Join(Environment.NewLine, logLines.ToArray());

                // Write to file
                File.WriteAllText(logFilePath, logContent, Encoding.UTF8);

                LogInfo("Generated log file: " + logFilePath);

                // Open in Notepad
                try
                {
                    Process.Start("notepad.exe", logFilePath);
                }
                catch (Exception ex)
                {
                    LogError("GenerateLog: Could not open Notepad: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                LogError("GenerateLog error: " + ex);
            }
        }

        private string GetRetargetHarmonyLogPath()
        {
            try
            {
                // Get the directory where this plugin (HarmonyDebugPanel) is located
                var thisAssembly = Assembly.GetExecutingAssembly();
                var thisDir = Path.GetDirectoryName(thisAssembly.Location);
                if (string.IsNullOrEmpty(thisDir))
                {
                    thisDir = Environment.CurrentDirectory;
                }

                // Find the game root by walking up directories until we find BepInEx or LobotomyCorp_Data
                var currentDir = thisDir;
                var gameRoot = thisDir;
                for (var i = 0; i < 10; i++)
                {
                    if (Directory.Exists(Path.Combine(currentDir, "BepInEx")) ||
                        Directory.Exists(Path.Combine(currentDir, "LobotomyCorp_Data")))
                    {
                        gameRoot = currentDir;
                        break;
                    }
                    var parent = Path.GetDirectoryName(currentDir);
                    if (string.IsNullOrEmpty(parent) || parent == currentDir)
                    {
                        break;
                    }
                    currentDir = parent;
                }

                // Now search for RetargetHarmony.log in common locations
                var searchPaths = new[]
                {
                    // Subdirectory structure: BepInEx/patchers/RetargetHarmony/logs/
                    Path.Combine(Path.Combine(Path.Combine(gameRoot, "BepInEx/patchers/RetargetHarmony"), "logs"), "RetargetHarmony.log"),
                    // Direct in patchers folder
                    Path.Combine(Path.Combine(gameRoot, "BepInEx/patchers"), "RetargetHarmony.log"),
                    // Also check in the same folder as this plugin (if it's in patchers)
                    Path.Combine(Path.Combine(thisDir, "logs"), "RetargetHarmony.log"),
                };

                foreach (var searchPath in searchPaths)
                {
                    LogInfo("GetRetargetHarmonyLogPath: Checking for RetargetHarmony log at: " + searchPath);
                    if (File.Exists(searchPath))
                    {
                        LogInfo("GetRetargetHarmonyLogPath: Found RetargetHarmony log at: " + searchPath);
                        return searchPath;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning("GetRetargetHarmonyLogPath: Error finding RetargetHarmony log: " + ex.Message);
            }

            return null;
        }

        private string GetBepInExLogPath()
        {
            try
            {
                // Get the directory where this plugin (HarmonyDebugPanel) is located
                var thisAssembly = Assembly.GetExecutingAssembly();
                var thisDir = Path.GetDirectoryName(thisAssembly.Location);
                if (string.IsNullOrEmpty(thisDir))
                {
                    thisDir = Environment.CurrentDirectory;
                }

                // Find the game root by walking up directories until we find BepInEx or LobotomyCorp_Data
                var currentDir = thisDir;
                var gameRoot = thisDir;
                for (var i = 0; i < 10; i++)
                {
                    var bepInExExists = Directory.Exists(currentDir + Path.DirectorySeparatorChar + "BepInEx");
                    var lobotomyDataExists = Directory.Exists(currentDir + Path.DirectorySeparatorChar + "LobotomyCorp_Data");

                    if (bepInExExists || lobotomyDataExists)
                    {
                        gameRoot = currentDir;
                        break;
                    }
                    var parent = Path.GetDirectoryName(currentDir);
                    if (string.IsNullOrEmpty(parent) || parent == currentDir)
                    {
                        break;
                    }
                    currentDir = parent;
                }

                // Check for BepInEx/LogOutput.log using explicit path construction
                var logPath = gameRoot + Path.DirectorySeparatorChar + "BepInEx" + Path.DirectorySeparatorChar + "LogOutput.log";
                LogInfo("GetBepInExLogPath: Checking for BepInEx log at: " + logPath);

                if (File.Exists(logPath))
                {
                    LogInfo("GetBepInExLogPath: Found BepInEx log at: " + logPath);
                    return logPath;
                }
                else
                {
                    LogWarning("GetBepInExLogPath: BepInEx log not found at: " + logPath);
                }
            }
            catch (Exception ex)
            {
                LogWarning("GetBepInExLogPath: Error finding BepInEx log: " + ex.Message);
            }

            return null;
        }

        private static string ReadLogFileSnapshot(string logPath)
        {
            try
            {
                using (var stream = new FileStream(
                    logPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetUnityLogPath()
        {
            try
            {
                // Unity log is at %USERPROFILE%\AppData\LocalLow\Project_Moon\Lobotomy\output_log.txt
                var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                if (string.IsNullOrEmpty(userProfile))
                {
                    // Try fallback for Unix-style environments
                    userProfile = Environment.GetEnvironmentVariable("HOME");
                }

                if (string.IsNullOrEmpty(userProfile))
                {
                    LogWarning("GetUnityLogPath: Could not find USERPROFILE or HOME environment variable");
                    return null;
                }

                var logPath = userProfile + Path.DirectorySeparatorChar + "AppData" + Path.DirectorySeparatorChar + "LocalLow" + Path.DirectorySeparatorChar + "Project_Moon" + Path.DirectorySeparatorChar + "Lobotomy" + Path.DirectorySeparatorChar + "output_log.txt";
                LogInfo("GetUnityLogPath: Checking for Unity log at: " + logPath);

                if (File.Exists(logPath))
                {
                    LogInfo("GetUnityLogPath: Found Unity log at: " + logPath);
                    return logPath;
                }
                else
                {
                    LogWarning("GetUnityLogPath: Unity log not found at: " + logPath);
                }
            }
            catch (Exception ex)
            {
                LogWarning("GetUnityLogPath: Error finding Unity log: " + ex.Message);
            }

            return null;
        }
    }
}
