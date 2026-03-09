// SPDX-License-Identifier: MIT

using System.Text.Json;

namespace CI
{
    internal interface ICoverageConfigReader
    {
        CoverageConfig? ReadConfig(string repoRoot);
    }

    internal sealed class CoverageConfigReader(IFileSystem fileSystem) : ICoverageConfigReader
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public CoverageConfigReader()
            : this(new FileSystem())
        {
        }

        public CoverageConfig? ReadConfig(string repoRoot)
        {
            string configPath = System.IO.Path.Combine(repoRoot, "coverlet.json");

            if (!_fileSystem.FileExists(configPath))
            {
                return null;
            }

            string? json = _fileSystem.ReadAllText(configPath);

            if (json == null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<CoverageConfig>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
