// SPDX-License-Identifier: MIT

using System.IO;

namespace LobotomyPlaywright.Interfaces.Configuration;

/// <summary>
/// Interface for managing the LobotomyPlaywright configuration.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Loads the configuration from disk.
    /// </summary>
    /// <returns>The configuration object.</returns>
    /// <exception cref="FileNotFoundException">Thrown when config file doesn't exist.</exception>
    Config Load();

    /// <summary>
    /// Saves the configuration to disk.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    void Save(Config config);
}

/// <summary>
/// Configuration for LobotomyPlaywright.
/// </summary>
public class Config
{
    /// <summary>
    /// Gets or sets the game installation path.
    /// </summary>
    public string GamePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CrossOver bottle name (macOS only).
    /// </summary>
    public string? CrossoverBottle { get; set; }

    /// <summary>
    /// Gets or sets the TCP port for the plugin.
    /// </summary>
    public int TcpPort { get; set; } = 8484;

    /// <summary>
    /// Gets or sets the launch timeout in seconds.
    /// </summary>
    public int LaunchTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the shutdown timeout in seconds.
    /// </summary>
    public int ShutdownTimeoutSeconds { get; set; } = 10;
}
