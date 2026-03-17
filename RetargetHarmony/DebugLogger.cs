// SPDX-License-Identifier: MIT

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;

namespace RetargetHarmony
{
    /// <summary>
    /// Configurable debug logger with multiple output targets.
    /// </summary>
    /// <remarks>
    /// Lazy-initializes on first use. When RetargetHarmony.cfg is present in the assembly directory,
    /// debug logging is enabled at the configured level. Without config file, only Info/Warn/Error
    /// messages go to BepInEx (existing behavior).
    /// </remarks>
    public static class DebugLogger
    {
        private const string ConfigFileName = "RetargetHarmony.cfg";
        private const string LogFolderName = "logs";
        private const string LogFileName = "RetargetHarmony.log";

        /// <summary>
        /// Log level enumeration.
        /// </summary>
        public enum LogLevel
        {
            None = 0,
            Trace,
            Debug,
            Info,
            Warn,
            Error,
        }

        // Static state
        private static bool s_initialized;
        private static bool s_debugEnabled;
        private static LogLevel s_minLevel = LogLevel.Debug; // Default if config present but unparseable
        private static string s_logFilePath;
        private static ManualLogSource s_bepInExLog;
        private static bool s_unityAvailable = true;
        private static readonly object s_fileLock = new object();

        // Testability: override config directory
        public static string ConfigDirectoryOverride { get; set; }

        // Testability: override log file path
        public static string LogFilePathOverride { get; set; }

        /// <summary>
        /// Initializes the debug logger. Lazy-initializes on first log call.
        /// </summary>
        /// <param name="log">BepInEx ManualLogSource for logging.</param>
        public static void Initialize(ManualLogSource log)
        {
            if (s_initialized)
            {
                return;
            }

            s_bepInExLog = log;

            // Determine config directory - check assembly directory first (patcher's own folder),
            // then fall back to BepInEx config path
            // This ensures patcher logs go in the patcher folder, not the shared config folder
            string configPath;

            // First check the patcher's own directory (assembly location)
            var assemblyDir = ConfigDirectoryOverride ?? GetAssemblyDirectory();
            var assemblyConfigPath = Path.Combine(assemblyDir, ConfigFileName);

            if (File.Exists(assemblyConfigPath))
            {
                // Config found in assembly directory - use that
                configPath = assemblyConfigPath;
            }
            else if (!string.IsNullOrEmpty(Paths.ConfigPath))
            {
                // Fall back to BepInEx config path
                configPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
                if (File.Exists(configPath))
                {
                    _ = Paths.ConfigPath;
                }
                else
                {
                    // No config file exists yet - use assembly directory for the default config
                    configPath = assemblyConfigPath;
                }
            }
            else
            {
                // No BepInEx paths available - use assembly directory
                configPath = assemblyConfigPath;
            }

            if (File.Exists(configPath))
            {
                s_debugEnabled = true;
                ParseConfigFile(configPath);
            }
            else
            {
                // Default to Warn level (warnings and errors) when no config file exists
                // This ensures users can see important issues without needing to create a config
                s_debugEnabled = true;
                s_minLevel = LogLevel.Warn;
            }

            // Set up log file path - ALWAYS use the patcher's own folder (assembly directory)
            // for logs, regardless of where the config file is found
            if (LogFilePathOverride != null)
            {
                s_logFilePath = LogFilePathOverride;
            }
            else if (s_debugEnabled)
            {
                var logDir = Path.Combine(assemblyDir, LogFolderName);
                if (!Directory.Exists(logDir))
                {
                    _ = Directory.CreateDirectory(logDir);
                }

                s_logFilePath = Path.Combine(logDir, LogFileName);

                // Clear existing log file on each launch to avoid accumulating logs
                try
                {
                    File.WriteAllText(s_logFilePath, string.Empty);
                }
                catch (IOException)
                {
                    // Silently ignore if clear fails (file locked, etc.)
                }
            }

            s_initialized = true;
        }

        /// <summary>
        /// Resets all static state for test isolation.
        /// </summary>
        public static void Reset()
        {
            s_initialized = false;
            s_debugEnabled = false;
            s_minLevel = LogLevel.Debug;
            s_logFilePath = null;
            s_bepInExLog = null;
            s_unityAvailable = true;
            ConfigDirectoryOverride = null;
            LogFilePathOverride = null;
        }

        /// <summary>
        /// Sets the log level from config file value.
        /// Called by RetargetHarmony.InitializeConfig when parsing the shared config file.
        /// </summary>
        /// <param name="level">The log level string from config.</param>
        public static void SetLogLevelFromConfig(string level)
        {
            if (string.IsNullOrEmpty(level))
            {
                return;
            }

            try
            {
                LogLevel parsedLevel = (LogLevel)Enum.Parse(typeof(LogLevel), level, true);
                if (parsedLevel == LogLevel.None)
                {
                    s_debugEnabled = false;
                    s_minLevel = LogLevel.None;
                }
                else
                {
                    s_debugEnabled = true;
                    s_minLevel = parsedLevel;
                }
            }
            catch (ArgumentException)
            {
                // Invalid log level - ignore and keep current settings
            }
        }

        /// <summary>
        /// Logs a trace-level message.
        /// </summary>
        public static void Trace(string message)
        {
            Log(LogLevel.Trace, message);
        }

        /// <summary>
        /// Logs a debug-level message.
        /// </summary>
        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Logs an info-level message.
        /// </summary>
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a warning-level message.
        /// </summary>
        public static void Warn(string message)
        {
            Log(LogLevel.Warn, message);
        }

        /// <summary>
        /// Logs an error-level message.
        /// </summary>
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        private static void Log(LogLevel level, string message)
        {
            // Lazy initialization on first log call
            if (!s_initialized && s_bepInExLog != null)
            {
                Initialize(s_bepInExLog);
            }

            // Check if we should log based on level
            // Trace and Debug only log when debug is explicitly enabled
            // Info, Warn, Error always go to BepInEx (existing behavior)
            var shouldLogToFile = s_debugEnabled && level >= s_minLevel;
            var shouldLogToBepInEx = level >= LogLevel.Info;
            var shouldLogToUnity = s_debugEnabled && level >= s_minLevel && s_unityAvailable;

            if (shouldLogToFile)
            {
                WriteToFile(level, message);
            }

            if (shouldLogToBepInEx)
            {
                WriteToBepInEx(level, message);
            }

            if (shouldLogToUnity)
            {
                WriteToUnityDebug(level, message);
            }
        }

        private static void WriteToFile(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(s_logFilePath))
            {
                return;
            }

            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                var logLine = $"[{timestamp}] [{level.ToString().ToUpperInvariant()}] {message}{Environment.NewLine}";
                lock (s_fileLock)
                {
                    File.AppendAllText(s_logFilePath, logLine, Encoding.UTF8);
                }
            }
            catch (IOException)
            {
                // Silently ignore file write errors to prevent cascading failures
            }
        }

        private static void WriteToBepInEx(LogLevel level, string message)
        {
            if (s_bepInExLog == null)
            {
                return;
            }

            switch (level)
            {
                case LogLevel.Error:
                    s_bepInExLog.LogError(message);
                    break;
                case LogLevel.Warn:
                    s_bepInExLog.LogWarning(message);
                    break;
                case LogLevel.Info:
                    s_bepInExLog.LogInfo(message);
                    break;
                case LogLevel.None:
                case LogLevel.Trace:
                case LogLevel.Debug:
                default:
                    break;
            }
        }

        private static void WriteToUnityDebug(LogLevel level, string message)
        {
            // Unity Debug logging is disabled to prevent MissingMethodException errors
            // The Unity runtime version doesn't have the expected Debug methods
            // Commented out code below kept for reference if needed later:
            // try
            // {
            //     // Use type reference to avoid compile-time dependency issues
            //     var debugType = Type.GetType("UnityEngine.Debug, UnityEngine");
            //     if (debugType == null)
            //     {
            //         s_unityAvailable = false;
            //         return;
            //     }
            //
            //     var logMethod = level == LogLevel.Error
            //         ? debugType.GetMethod("LogError", new[] { typeof(object) })
            //         : level == LogLevel.Warn
            //             ? debugType.GetMethod("LogWarning", new[] { typeof(object) })
            //             : debugType.GetMethod("Log", new[] { typeof(object) });
            //
            //     logMethod?.Invoke(null, new[] { message });
            // }
            // catch
            // {
            //     // Mark as unavailable to avoid repeated exceptions
            //     s_unityAvailable = false;
            // }
        }

        private static void ParseConfigFile(string configPath)
        {
            try
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    // Skip comments and empty lines
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // Parse key=value
                    var equalsIndex = trimmed.IndexOf('=');
                    if (equalsIndex <= 0)
                    {
                        continue;
                    }

                    var key = trimmed.Substring(0, equalsIndex).Trim();
                    var value = trimmed.Substring(equalsIndex + 1).Trim();

                    if (key.Equals("LogLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            s_minLevel = (LogLevel)Enum.Parse(typeof(LogLevel), value, true);

                            // If LogLevel is None, disable debug logging entirely
                            s_debugEnabled = s_minLevel != LogLevel.None;
                        }
                        catch (ArgumentException)
                        {
                            // Default to Debug if unparseable
                            s_minLevel = LogLevel.Debug;
                            s_debugEnabled = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If config parsing fails, default to Debug level
                s_minLevel = LogLevel.Debug;
                s_debugEnabled = true;
            }
        }

        private static string GetAssemblyDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;

            // Handle cases where location might be empty (e.g., dynamic assembly)
            if (string.IsNullOrEmpty(location))
            {
                return Environment.CurrentDirectory;
            }

            // Find the directory part of the path
            var lastSep = Math.Max(location.LastIndexOf('/'), location.LastIndexOf('\\'));
            return lastSep > 0 ? location.Substring(0, lastSep) : Environment.CurrentDirectory;
        }
    }
}
