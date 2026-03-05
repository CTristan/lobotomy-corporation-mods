// SPDX-License-Identifier: MIT

#pragma warning disable CA1852, CA1515
using System.Text.Json;

namespace CI;

public interface ICoverageConfigReader
{
    CoverageConfig? ReadConfig(string repoRoot);
}

public sealed class CoverageConfigReader : ICoverageConfigReader
#pragma warning restore CA1852, CA1515
{
    private readonly IFileSystem _fileSystem;

    public CoverageConfigReader()
        : this(new FileSystem())
    {
    }

    public CoverageConfigReader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
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
