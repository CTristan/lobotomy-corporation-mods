// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using AwesomeAssertions;
using JetBrains.Annotations;
using Hemocode.Common.Implementations;
using Hemocode.Common.Implementations.Adapters;
using Hemocode.Common.Interfaces;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests
{
    public sealed class FileManagerTests
    {
        private const string DefaultModFileName = "LobotomyCorporationMods.Test.dll";

        [Fact]
        public void Able_to_find_existing_file_in_mod_folder()
        {
            FileManager fileManager = new(DefaultModFileName, GetDirectories());

            var result = fileManager.GetFile(DefaultModFileName);

            _ = result.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Adds_new_file_if_file_name_not_found_in_mod_folder()
        {
            FileManager fileManager = new(DefaultModFileName, GetDirectories());

            var result = fileManager.GetFile("NewFileName");

            _ = result.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("NonExistentFile.txt")]
        [InlineData("DoesNotExist")]
        public void Reading_a_nonexistent_file_with_flag_not_set_does_not_create_the_file([NotNull] string fileName)
        {
            FileManager fileManager = new(DefaultModFileName, GetDirectories());
            var fileWithPath = fileManager.GetFile(fileName);
            DeleteFileIfExists(fileWithPath);

            var result = fileManager.ReadAllText(fileWithPath, false);

            _ = result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("NowIExist.txt")]
        [InlineData("CreateThenDeleteMe")]
        public void Reading_a_nonexistent_file_with_flag_set_creates_the_file([NotNull] string fileName)
        {
            FileManager fileManager = new(DefaultModFileName, GetDirectories());
            var fileWithPath = fileManager.GetFile(fileName);
            DeleteFileIfExists(fileWithPath);

            _ = fileManager.ReadAllText(fileWithPath, true);

            _ = File.Exists(fileWithPath).Should().BeTrue();
        }

        [Fact]
        public void Throws_exception_when_unable_to_find_mod_folder()
        {
            Action action = () =>
            {
                _ = new FileManager(string.Empty, GetDirectories());
            };

            _ = action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Able_to_read_bytes_from_file()
        {
            FileManager fileManager = new(DefaultModFileName, GetDirectories());

            var result = fileManager.ReadAllBytes(DefaultModFileName);

            _ = result.Should().NotBeNull();
        }

        #region Helper Methods

        [NotNull]
        private static List<IDirectoryInfo> GetDirectories()
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            return
            [
                new DirectoryInfoAdapter(new DirectoryInfo(currentDirectory)),
            ];
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
