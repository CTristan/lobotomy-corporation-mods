// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using LobotomyCorporationMods.Common.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common
{
    public sealed class FileManagerTests
    {
        private const string DefaultModFileName = "LobotomyCorporationMods.Test.dll";

        [Fact]
        public void Able_to_find_existing_file_in_mod_folder()
        {
            var fileManager = new FileManager(DefaultModFileName, GetDirectories());

            var result = fileManager.GetOrCreateFile(DefaultModFileName);

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Adds_new_file_if_file_name_not_found_in_mod_folder()
        {
            var fileManager = new FileManager(DefaultModFileName, GetDirectories());

            var result = fileManager.GetOrCreateFile("NewFileName");

            result.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("NonExistentFile.txt")]
        [InlineData("DoesNotExist")]
        public void Reading_a_nonexistent_file_with_flag_not_set_does_not_create_the_file(string fileName)
        {
            var fileManager = new FileManager(DefaultModFileName, GetDirectories());
            var fileWithPath = fileManager.GetOrCreateFile(fileName);
            DeleteFileIfExists(fileWithPath);

            var result = fileManager.ReadAllText(fileWithPath, false);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("NowIExist.txt")]
        [InlineData("CreateThenDeleteMe")]
        public void Reading_a_nonexistent_file_with_flag_set_creates_the_file(string fileName)
        {
            var fileManager = new FileManager(DefaultModFileName, GetDirectories());
            var fileWithPath = fileManager.GetOrCreateFile(fileName);
            DeleteFileIfExists(fileWithPath);

            _ = fileManager.ReadAllText(fileWithPath, true);

            File.Exists(fileWithPath).Should().BeTrue();
        }

        [Fact]
        public void Throws_exception_when_unable_to_find_mod_folder()
        {
            Action action = () => _ = new FileManager(string.Empty, GetDirectories());

            action.Should().Throw<InvalidOperationException>();
        }

        #region Helper Methods

        private static ICollection<DirectoryInfo> GetDirectories()
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            return new List<DirectoryInfo> { new(currentDirectory) };
        }

        private static void DeleteFileIfExists(string fileWithPath)
        {
            if (File.Exists(fileWithPath))
            {
                File.Delete(fileWithPath);
            }
        }

        #endregion
    }
}
