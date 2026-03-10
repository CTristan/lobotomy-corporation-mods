// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Configuration;

/// <summary>
/// Manages loading and saving of the LobotomyPlaywright configuration.
/// </summary>
internal sealed class ConfigManager : IConfigManager
{
    private readonly IFileSystem _fileSystem;
    private readonly string _configPath;

    /// <summary>
    /// Initializes a new instance of the ConfigManager class.
    /// </summary>
    /// <param name="fileSystem">The file system implementation.</param>
    /// <param name="configPath">The path to the config file.</param>
    public ConfigManager(IFileSystem fileSystem, string configPath)
    {
        _fileSystem = fileSystem;
        _configPath = configPath;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigManager class with default config path.
    /// </summary>
    /// <param name="fileSystem">The file system implementation.</param>
    public ConfigManager(IFileSystem fileSystem)
        : this(fileSystem, GetDefaultConfigPath())
    {
    }

    /// <summary>
    /// Gets the default configuration file path.
    /// </summary>
    /// <returns>The path to config.json.</returns>
    public static string GetDefaultConfigPath()
    {
        var repoRoot = FindRepositoryRoot();
        if (repoRoot == null)
        {
            throw new InvalidOperationException("Could not find repository root. Are you in a git repository?");
        }

        return Path.Combine(repoRoot, ".pi", "skills", "lobotomy-playwright", "config.json");
    }

    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            if (File.Exists(Path.Combine(dir.FullName, "LobotomyCorporationMods.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    public Config Load()
    {
        if (!_fileSystem.FileExists(_configPath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {_configPath}\n" +
                $"Run 'dotnet playwright find-game' to auto-detect game path.");
        }

        var json = _fileSystem.ReadAllText(_configPath);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException($"Configuration file is empty: {_configPath}");
        }

        try
        {
            var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return config ?? throw new InvalidOperationException("Failed to deserialize configuration");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in config file: {ex.Message}", ex);
        }
    }

    public void Save(Config config)
    {
        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
        {
            _fileSystem.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(config, options);
        _fileSystem.WriteAllText(_configPath, json);
    }
}
