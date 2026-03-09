// SPDX-License-Identifier: MIT

using System;
using System.IO;
using LobotomyPlaywright.Implementations.Configuration;
using LobotomyPlaywright.Implementations.Network;
using LobotomyPlaywright.Implementations.System;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Network;

namespace LobotomyPlaywright.Commands;

/// <summary>
/// Command to capture a screenshot of the current game state.
/// </summary>
public class ScreenshotCommand
{
    private readonly IConfigManager _configManager;
    private readonly Func<ITcpClient> _tcpClientFactory;

    /// <summary>
    /// Initializes a new instance of ScreenshotCommand class.
    /// </summary>
    /// <param name="configManager">The config manager.</param>
    /// <param name="tcpClientFactory">Factory for creating TCP clients.</param>
    public ScreenshotCommand(IConfigManager configManager, Func<ITcpClient> tcpClientFactory)
    {
        _configManager = configManager;
        _tcpClientFactory = tcpClientFactory;
    }

    /// <summary>
    /// Initializes a new instance of ScreenshotCommand class with default implementations.
    /// </summary>
    public ScreenshotCommand()
        : this(new ConfigManager(new FileSystem()), () => new PlaywrightTcpClient())
    {
    }

    /// <summary>
    /// Runs the screenshot command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <returns>Exit code (0 for success, non-zero for failure).</returns>
    public int Run(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var formatArg = GetArgValue(args, "--format") ?? "base64";
        var outputPath = GetArgValue(args, "--output");
        var displayFormat = GetArgValue(args, "--display") ?? "text";
        var host = GetArgValue(args, "--host") ?? "localhost";
        var portArg = GetArgValue(args, "--port");

        // Validate format argument
        formatArg = formatArg.ToLowerInvariant();
        if (formatArg != "base64" && formatArg != "path")
        {
            Console.Error.WriteLine($"ERROR: Invalid format '{formatArg}'. Must be 'base64' or 'path'.");
            return 1;
        }

        // Validate display format argument
        displayFormat = displayFormat.ToLowerInvariant();
        if (displayFormat != "text" && displayFormat != "json")
        {
            Console.Error.WriteLine($"ERROR: Invalid display format '{displayFormat}'. Must be 'text' or 'json'.");
            return 1;
        }

        // Load configuration
        Config config;
        try
        {
            config = _configManager.Load();
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 1;
        }

        var port = portArg != null ? int.Parse(portArg) : config.TcpPort;

        try
        {
            using var client = _tcpClientFactory();
            client.Connect(host, port);

            // Build command parameters
            var parameters = new System.Collections.Generic.Dictionary<string, object>
            {
                { "format", formatArg }
            };

            // Send screenshot command
            var responseData = client.SendCommandWithData("screenshot", parameters);

            if (responseData == null)
            {
                Console.Error.WriteLine("ERROR: No response from server");
                return 1;
            }

            // Extract response data
            if (!responseData.TryGetValue("filename", out var filenameObj) || filenameObj == null)
            {
                Console.Error.WriteLine("ERROR: Response missing filename");
                return 1;
            }

            string filename = filenameObj.ToString() ?? string.Empty;
            string path = responseData.TryGetValue("path", out var pathObj) ? GetStringValue(pathObj) ?? string.Empty : string.Empty;
            long size = responseData.TryGetValue("size", out var sizeObj) && sizeObj != null
                ? GetLongValue(sizeObj)
                : 0;
            string timestamp = responseData.TryGetValue("timestamp", out var timestampObj)
                ? GetStringValue(timestampObj) ?? string.Empty
                : string.Empty;

            // Handle base64 data if present
            string? base64Data = null;
            if (formatArg == "base64" && responseData.TryGetValue("base64", out var base64Obj) && base64Obj != null)
            {
                base64Data = GetStringValue(base64Obj);
            }

            // If output path specified, save the image
            string savedPath = path;
            if (!string.IsNullOrEmpty(outputPath) && !string.IsNullOrEmpty(base64Data))
            {
                try
                {
                    var imageBytes = Convert.FromBase64String(base64Data);
                    File.WriteAllBytes(outputPath, imageBytes);
                    savedPath = Path.GetFullPath(outputPath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERROR: Failed to save image to '{outputPath}': {ex.Message}");
                    return 1;
                }
            }

            // Display results
            if (displayFormat == "json")
            {
                DisplayJsonResponse(filename, savedPath, size, timestamp, base64Data);
            }
            else
            {
                DisplayTextResponse(filename, savedPath, size, timestamp, base64Data);
            }

            return 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.Net.Sockets.SocketException)
        {
            Console.Error.WriteLine($"Connection error: {ex.Message}");
            Console.Error.WriteLine("Ensure Lobotomy Corporation is running with LobotomyPlaywright plugin.");
            return 1;
        }
    }

    private static void DisplayTextResponse(string filename, string path, long size, string timestamp, string? base64Data)
    {
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine("Screenshot Captured");
        Console.WriteLine("".PadRight(60, '='));
        Console.WriteLine($"  Filename: {filename}");
        Console.WriteLine($"  Path: {path}");
        Console.WriteLine($"  Size: {size:N0} bytes");
        Console.WriteLine($"  Timestamp (UTC): {timestamp}");

        if (!string.IsNullOrEmpty(base64Data))
        {
            var truncatedBase64 = base64Data.Length > 100
                ? string.Concat(base64Data.AsSpan(0, 100), "...")
                : base64Data;
            Console.WriteLine($"  Base64 (truncated): {truncatedBase64}");
            Console.WriteLine();
            Console.WriteLine("To decode and display the image:");
            Console.WriteLine($"  echo \"{base64Data}\" | base64 -d > screenshot.png");
            Console.WriteLine($"  open screenshot.png  # macOS");
        }

        Console.WriteLine("".PadRight(60, '='));
    }

    private static void DisplayJsonResponse(string filename, string path, long size, string timestamp, string? base64Data)
    {
        var response = new System.Collections.Generic.Dictionary<string, object>
        {
            { "status", "ok" },
            { "filename", filename },
            { "path", path },
            { "size", size },
            { "timestamp", timestamp }
        };

        if (!string.IsNullOrEmpty(base64Data))
        {
            response.Add("base64", base64Data);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    private static string? GetArgValue(string[] args, string argName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == argName && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static bool HasArg(string[] args, string argName)
    {
        foreach (var arg in args)
        {
            if (arg == argName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Safely extracts a string value from an object (handles JsonElement).
    /// </summary>
    private static string? GetStringValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is string stringValue)
        {
            return stringValue;
        }

        // Handle System.Text.Json.JsonElement
        var valueType = value.GetType();
        if (valueType.FullName != null && valueType.FullName.Contains("JsonElement", StringComparison.OrdinalIgnoreCase))
        {
            return value.ToString();
        }

        return value.ToString();
    }

    /// <summary>
    /// Safely extracts a long value from an object (handles JsonElement).
    /// </summary>
    private static long GetLongValue(object value)
    {
        if (value == null)
        {
            return 0;
        }

        if (value is long longValue)
        {
            return longValue;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        // Handle System.Text.Json.JsonElement - re-serialize and deserialize as specific type
        var valueType = value.GetType();
        if (valueType.FullName != null && valueType.FullName.Contains("JsonElement", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var elementString = System.Text.Json.JsonSerializer.Serialize(value);
                return System.Text.Json.JsonSerializer.Deserialize<long>(elementString);
            }
            catch
            {
                // Fall through to default
            }
        }

        // Last resort: try Convert with a try-catch
        try
        {
            return Convert.ToInt64(value);
        }
        catch
        {
            return 0;
        }
    }
}
