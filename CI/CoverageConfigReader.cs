// SPDX-License-Identifier: MIT

using System.Text.Json;

namespace CI
{
    public interface ICoverageConfigReader
    {
        CoverageConfig? ReadConfig(string repoRoot);
    }

    public sealed class CoverageConfigReader(IFileSystem fileSystem) : ICoverageConfigReader
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public CoverageConfigReader()
            : this(new FileSystem())
        {
        }

        public CoverageConfig? ReadConfig(string repoRoot)
        {
            var configPath = System.IO.Path.Combine(repoRoot, "coverlet.json");

            if (!_fileSystem.FileExists(configPath))
            {
                return null;
            }

            var json = _fileSystem.ReadAllText(configPath);

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
