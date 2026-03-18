// SPDX-License-Identifier: MIT

using System;
using System.IO;
using AwesomeAssertions;
using Harmony2ForLmm.Services;
using Xunit;

namespace Harmony2ForLmm.Test.Services
{
    /// <summary>
    /// Tests for <see cref="GameDirectoryValidator"/>.
    /// </summary>
    public sealed class GameDirectoryValidatorTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly GameDirectoryValidator _validator;

        public GameDirectoryValidatorTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _validator = new GameDirectoryValidator();
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Fact]
        public void Empty_path_returns_failure()
        {
            var result = _validator.Validate(string.Empty);

            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("empty");
        }

        [Fact]
        public void Null_path_returns_failure()
        {
            var result = _validator.Validate(null!);

            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("empty");
        }

        [Fact]
        public void Nonexistent_directory_returns_failure()
        {
            var result = _validator.Validate(Path.Combine(_tempDir, "nonexistent"));

            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("does not exist");
        }

        [Fact]
        public void Directory_without_Assembly_CSharp_returns_failure()
        {
            var result = _validator.Validate(_tempDir);

            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Assembly-CSharp.dll");
        }

        [Fact]
        public void Valid_game_directory_returns_success()
        {
            var managedDir = Path.Combine(_tempDir, "LobotomyCorp_Data", "Managed");
            Directory.CreateDirectory(managedDir);
            File.WriteAllBytes(Path.Combine(managedDir, "Assembly-CSharp.dll"), [0]);

            var result = _validator.Validate(_tempDir);

            result.IsValid.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
        }
    }
}
