// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LobotomyPlaywright.Interfaces.Configuration;
using LobotomyPlaywright.Interfaces.Deployment;
using LobotomyPlaywright.Interfaces.System;

namespace LobotomyPlaywright.Implementations.Deployment
{
    /// <summary>
    /// Loads deployment profiles from an external JSON file.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ProfileLoader class.
    /// </remarks>
    /// <param name="fileSystem">The file system implementation.</param>
    /// <param name="profilesPath">The path to the profiles JSON file.</param>
    public sealed class ProfileLoader(IFileSystem fileSystem, string profilesPath) : IProfileLoader
    {
        private readonly IFileSystem _fileSystem = fileSystem;
        private readonly string _profilesPath = profilesPath;

        public Dictionary<string, DeploymentProfile> Load()
        {
            if (!_fileSystem.FileExists(_profilesPath))
            {
                throw new FileNotFoundException($"Profiles file not found: {_profilesPath}");
            }

            var json = _fileSystem.ReadAllText(_profilesPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException($"Profiles file is empty: {_profilesPath}");
            }

            try
            {
                var profiles = JsonSerializer.Deserialize<Dictionary<string, DeploymentProfile>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return profiles ?? throw new InvalidOperationException("Failed to deserialize profiles");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON in profiles file: {ex.Message}", ex);
            }
        }
    }
}
