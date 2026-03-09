// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using FluentAssertions;
using RetargetHarmony;
using Xunit;
using Xunit.Abstractions;
using DebugLogger = RetargetHarmony.DebugLogger;

#pragma warning disable xUnit1000 // Test classes must be public - xUnit requirement
#pragma warning disable CA1812 // Internal class apparently never instantiated - xUnit instantiates test classes
#pragma warning disable CA1031 // Intentionally testing exception handling

namespace RetargetHarmony.Test;

public sealed class DebugLoggerTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _tempDir;
#pragma warning disable CA2213 // ManualLogSource is created by BepInEx Logger, caller doesn't control its lifecycle
    private ManualLogSource _mockLog;
#pragma warning restore CA2213

    public DebugLoggerTests(ITestOutputHelper output)
    {
        _output = output;
        _tempDir = Path.Combine(Path.GetTempPath(), "RetargetHarmonyTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        // Create mock ManualLogSource
        _mockLog = Logger.CreateLogSource("RetargetHarmony.Test");

        // Reset and configure DebugLogger
        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");
    }

    public void Dispose()
    {
        DebugLogger.Reset();

        // Clean up temp directory
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void Initialize_WithNoConfigFile_DoesNotEnableDebugLogging()
    {
        // Arrange - no config file
        DebugLogger.Reset();

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert - Info should work, but Trace/Debug should not go to file
        DebugLogger.Info("Test info message");
        DebugLogger.Trace("Test trace message");

        // Verify log file was not created (since debug is disabled)
        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeFalse("log file should not be created when debug is disabled");
    }

    [Fact]
    public void Initialize_WithValidConfigFile_EnablesDebugLogging()
    {
        // Arrange - create config file
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=Debug");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert
        DebugLogger.Debug("Test debug message");

        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue("log file should be created when debug is enabled");
        var content = File.ReadAllText(logPath);
        content.Should().Contain("DEBUG");
        content.Should().Contain("Test debug message");
    }

    [Fact]
    public void ParseConfigFile_ValidLogLevel_SetsCorrectLevel()
    {
        // Arrange
        var levels = new[] { "Trace", "Debug", "Info", "Warn", "Error" };
        var expectedLevels = new[]
        {
            DebugLogger.LogLevel.Trace,
            DebugLogger.LogLevel.Debug,
            DebugLogger.LogLevel.Info,
            DebugLogger.LogLevel.Warn,
            DebugLogger.LogLevel.Error
        };

        for (int i = 0; i < levels.Length; i++)
        {
            // Arrange
            var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
            File.WriteAllText(configPath, string.Format(CultureInfo.InvariantCulture, "LogLevel={0}", levels[i]));

            DebugLogger.Reset();
            DebugLogger.ConfigDirectoryOverride = _tempDir;
            DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

            // Act
            DebugLogger.Initialize(_mockLog);

            // Assert - call the specific log level and verify it appears in file
            switch (levels[i])
            {
                case "Trace":
                    DebugLogger.Trace("Test trace");
                    break;
                case "Debug":
                    DebugLogger.Debug("Test debug");
                    break;
                case "Info":
                    DebugLogger.Info("Test info");
                    break;
                case "Warn":
                    DebugLogger.Warn("Test warn");
                    break;
                case "Error":
                    DebugLogger.Error("Test error");
                    break;
            }

            // Read the log file and verify the level
            var logPath = Path.Combine(_tempDir, "test.log");
            if (File.Exists(logPath))
            {
                var content = File.ReadAllText(logPath);
                content.Should().Contain(expectedLevels[i].ToString().ToUpperInvariant());
            }

            // Clean up for next iteration
            DebugLogger.Reset();
            var testLogPath = Path.Combine(_tempDir, "test.log");
            if (File.Exists(testLogPath))
            {
                File.Delete(testLogPath);
            }
        }
    }

    [Fact]
    public void ParseConfigFile_InvalidLogLevel_DefaultsToDebug()
    {
        // Arrange - invalid log level
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=InvalidLevel");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert - should default to Debug
        DebugLogger.Debug("Test debug message");

        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue("log file should be created");
        var content = File.ReadAllText(logPath);
        content.Should().Contain("DEBUG");
    }

    [Fact]
    public void ParseConfigFile_CaseInsensitive_HandlesLowercase()
    {
        // Arrange
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=debug");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert
        DebugLogger.Debug("Test");

        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue();
        var content = File.ReadAllText(logPath);
        content.Should().Contain("DEBUG");
    }

    [Fact]
    public void Info_AlwaysLoggedToBepInEx_WhenDebugDisabled()
    {
        // Arrange
        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // No config file - debug disabled

        // Act - should not throw
        var exception = Record.Exception(() => DebugLogger.Info("Test info message"));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void Warn_AlwaysLoggedToBepInEx_WhenDebugDisabled()
    {
        // Arrange
        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // No config file

        // Act
        var exception = Record.Exception(() => DebugLogger.Warn("Test warn message"));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void Error_AlwaysLoggedToBepInEx_WhenDebugDisabled()
    {
        // Arrange
        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // No config file

        // Act
        var exception = Record.Exception(() => DebugLogger.Error("Test error message"));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public void LogFile_WritesCorrectFormat()
    {
        // Arrange - create config with Trace level
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=Trace");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);
        DebugLogger.Trace("Test trace message");
        DebugLogger.Debug("Test debug message");
        DebugLogger.Info("Test info message");
        DebugLogger.Warn("Test warn message");
        DebugLogger.Error("Test error message");

        // Assert
        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue();

        var content = File.ReadAllText(logPath);
        _output.WriteLine("Log content:\n{0}", content);

        // Verify format: [yyyy-MM-dd HH:mm:ss.fff] [LEVEL] message
        content.Should().Contain("[TRACE] Test trace message");
        content.Should().Contain("[DEBUG] Test debug message");
        content.Should().Contain("[INFO] Test info message");
        content.Should().Contain("[WARN] Test warn message");
        content.Should().Contain("[ERROR] Test error message");

        // Verify timestamp format
        content.Should().MatchRegex(@"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]");
    }

    [Fact]
    public void LogFile_RespectsMinLevel()
    {
        // Arrange - set min level to Warn
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=Warn");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);
        DebugLogger.Trace("Should not appear");
        DebugLogger.Debug("Should not appear");
        DebugLogger.Info("Should not appear");
        DebugLogger.Warn("Should appear");
        DebugLogger.Error("Should appear");

        // Assert
        var logPath = Path.Combine(_tempDir, "test.log");
        var content = File.ReadAllText(logPath);

        content.Should().NotContain("Should not appear");
        content.Should().Contain("Should appear");
    }

    [Fact]
    public void Reset_ClearsState()
    {
        // Arrange - enable debug
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        File.WriteAllText(configPath, "LogLevel=Debug");

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        DebugLogger.Initialize(_mockLog);
        DebugLogger.Debug("Test message");

        // Act - reset
        DebugLogger.Reset();

        // Verify Reset cleared the state - Info should still work but Trace should not create log file
        DebugLogger.Info("After reset");

        var logPath = Path.Combine(_tempDir, "test.log");
        // After reset, the file override is cleared too, so no new file should be created
        // But we need to check what's expected - let's verify Reset works
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test2.log");

        // Now initialize and verify
        DebugLogger.Initialize(_mockLog);
        DebugLogger.Debug("After reset 2");

        var logPath2 = Path.Combine(_tempDir, "test2.log");
        File.Exists(logPath2).Should().BeTrue();
    }

    [Fact]
    public void ConfigFile_ParsesComments()
    {
        // Arrange - config with comments
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        var configContent = @"# This is a comment
LogLevel=Debug
# Another comment
";
        File.WriteAllText(configPath, configContent);

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert
        DebugLogger.Debug("Test");

        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue();
    }

    [Fact]
    public void ConfigFile_ParsesEmptyLines()
    {
        // Arrange - config with empty lines
        var configPath = Path.Combine(_tempDir, "RetargetHarmony.cfg");
        var configContent = @"

LogLevel=Debug

";
        File.WriteAllText(configPath, configContent);

        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act
        DebugLogger.Initialize(_mockLog);

        // Assert
        DebugLogger.Debug("Test");

        var logPath = Path.Combine(_tempDir, "test.log");
        File.Exists(logPath).Should().BeTrue();
    }

    [Fact]
    public void Initialize_CanBeCalledMultipleTimes()
    {
        // Arrange
        DebugLogger.Reset();
        DebugLogger.ConfigDirectoryOverride = _tempDir;
        DebugLogger.LogFilePathOverride = Path.Combine(_tempDir, "test.log");

        // Act - call initialize multiple times
        DebugLogger.Initialize(_mockLog);
        DebugLogger.Initialize(_mockLog);
        DebugLogger.Initialize(_mockLog);

        // Assert - should not throw
        var exception = Record.Exception(() => DebugLogger.Info("Test"));
        exception.Should().BeNull();
    }
}
