// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Configuration
{
    /// <summary>
    /// Manages loading and saving of the LobotomyPlaywright configuration.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ConfigManager class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    /// <param name="configPath">The path to the config file.</param>
    public sealed class ConfigManager(IFileSystem fileSystem, string configPath) : IConfigManager
    {
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly string _configPath = configPath;

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
            var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("Could not find repository root. Are you in a git repository?");

            // Prefer .playwright/config.json, fall back to legacy .pi path
            var newPath = Path.Combine(repoRoot, ".playwright", "config.json");
            if (File.Exists(newPath))
            {
                return newPath;
            }

            var legacyPath = Path.Combine(repoRoot, ".pi", "skills", "lobotomy-playwright", "config.json");
            if (File.Exists(legacyPath))
            {
                return legacyPath;
            }

            // Return new path as default (will surface a clear error if missing)
            return newPath;
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
}
