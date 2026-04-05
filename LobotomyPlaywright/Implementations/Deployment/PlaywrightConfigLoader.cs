// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Loads the full playwright configuration from playwright.json.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the PlaywrightConfigLoader class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    /// <param name="configPath">The path to the playwright.json file.</param>
    public sealed class PlaywrightConfigLoader(IFileSystem fileSystem, string configPath) : IPlaywrightConfigLoader
    {
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly string _configPath = configPath;

        public PlaywrightConfig Load()
        {
            if (!_fileSystem.FileExists(_configPath))
            {
                throw new FileNotFoundException($"Playwright config file not found: {_configPath}");
            }

            var json = _fileSystem.ReadAllText(_configPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException($"Playwright config file is empty: {_configPath}");
            }

            try
            {
                var config = JsonSerializer.Deserialize<PlaywrightConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return config ?? throw new InvalidOperationException("Failed to deserialize playwright config");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON in playwright config file: {ex.Message}", ex);
            }
        }
    }
}
