// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using AwesomeAssertions;
using LobotomyCorporationMods.DontChatMe.Configuration;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.ConfigTests
{
    /// <summary>
    ///     Verifies that <c>DontChatMeConfig</c> loads persisted values from an INI config file
    ///     on construction and writes them back via <c>Save()</c>. The read-modify-write invariant
    ///     ensures that sections other than [Connection] are preserved across a Save call, which
    ///     protects ConfigurationManager-written entries from being silently dropped.
    /// </summary>
    public sealed class DontChatMeConfigPersistenceTests : IDisposable
    {
        private readonly string _tempFile;

        public DontChatMeConfigPersistenceTests()
        {
            _tempFile = Path.GetTempFileName();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }

        private DontChatMeConfig BuildConfig(string contents = null)
        {
            if (contents != null)
            {
                File.WriteAllText(_tempFile, contents);
            }

            return new DontChatMeConfig(_tempFile);
        }

        [Fact]
        public void Constructor_loads_ServerUrl_from_config_file()
        {
            var config = BuildConfig("[Connection]\nServerUrl = ws://127.0.0.1:8585/mod/socket/\n");

            config.ServerUrl.Should().Be(new Uri("ws://127.0.0.1:8585/mod/socket/"));
        }

        [Fact]
        public void Constructor_loads_AuthToken_from_config_file()
        {
            var config = BuildConfig("[Connection]\nAuthToken = my-secret-token\n");

            config.AuthToken.Should().Be("my-secret-token");
        }

        [Fact]
        public void Constructor_loads_Enabled_false_from_config_file()
        {
            var config = BuildConfig("[Connection]\nEnabled = False\n");

            config.Enabled.Should().BeFalse();
        }

        [Fact]
        public void Constructor_with_nonexistent_file_uses_defaults()
        {
            File.Delete(_tempFile);

            var config = new DontChatMeConfig(_tempFile);

            config.ServerUrl.Should().BeNull();
            config.AuthToken.Should().BeEmpty();
            config.Enabled.Should().BeTrue();
        }

        [Fact]
        public void Constructor_ignores_comment_lines_in_config_file()
        {
            var config = BuildConfig(
                "[Connection]\n## Description\n# Type: String\nServerUrl = ws://example.com/socket\n"
            );

            config.ServerUrl.Should().Be(new Uri("ws://example.com/socket"));
        }

        [Fact]
        public void Save_writes_ServerUrl_to_config_file()
        {
            var config = BuildConfig();
            config.ServerUrl = new Uri("ws://127.0.0.1:8585/mod/socket/");

            config.Save();

            File.ReadAllText(_tempFile)
                .Should()
                .Contain("ServerUrl = ws://127.0.0.1:8585/mod/socket/");
        }

        [Fact]
        public void Save_writes_AuthToken_to_config_file()
        {
            var config = BuildConfig();
            config.AuthToken = "tok-42";

            config.Save();

            File.ReadAllText(_tempFile).Should().Contain("AuthToken = tok-42");
        }

        [Fact]
        public void Save_writes_Enabled_false_to_config_file()
        {
            var config = BuildConfig();
            config.Enabled = false;

            config.Save();

            File.ReadAllText(_tempFile).Should().Contain("Enabled = False");
        }

        [Fact]
        public void Save_then_load_round_trips_all_three_connection_values()
        {
            var config = BuildConfig();
            config.ServerUrl = new Uri("ws://127.0.0.1:8585/mod/socket/");
            config.AuthToken = "round-trip-token";
            config.Enabled = false;
            config.Save();

            var reloaded = new DontChatMeConfig(_tempFile);

            reloaded.ServerUrl.Should().Be(new Uri("ws://127.0.0.1:8585/mod/socket/"));
            reloaded.AuthToken.Should().Be("round-trip-token");
            reloaded.Enabled.Should().BeFalse();
        }

        [Fact]
        public void Save_preserves_non_Connection_sections_in_the_file()
        {
            var config = BuildConfig("[Effects]\nDangerEffectsEnabled = True\n");
            config.ServerUrl = new Uri("ws://example.com/socket");

            config.Save();

            File.ReadAllText(_tempFile).Should().Contain("DangerEffectsEnabled = True");
        }

        [Fact]
        public void Save_updates_existing_key_rather_than_adding_a_duplicate()
        {
            var config = BuildConfig("[Connection]\nServerUrl = ws://old.example.com/socket\n");
            config.ServerUrl = new Uri("ws://new.example.com/socket");

            config.Save();

            var written = File.ReadAllText(_tempFile);
            written.Should().Contain("ServerUrl = ws://new.example.com/socket");
            written.Should().NotContain("ws://old.example.com/socket");
        }

        [Fact]
        public void Save_creates_config_file_when_it_does_not_exist()
        {
            File.Delete(_tempFile);
            var config = new DontChatMeConfig(_tempFile);
            config.ServerUrl = new Uri("ws://127.0.0.1:8585/mod/socket/");

            config.Save();

            File.Exists(_tempFile).Should().BeTrue();
        }
    }
}
