// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using Hemocode.Common.Models.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class ErrorLogEntryTests
    {
        [Fact]
        public void Constructor_stores_all_properties()
        {
            var entry = new ErrorLogEntry("Herror.txt", "error content", "/path/Herror.txt");

            entry.FileName.Should().Be("Herror.txt");
            entry.Content.Should().Be("error content");
            entry.FilePath.Should().Be("/path/Herror.txt");
        }

        [Fact]
        public void Constructor_throws_when_fileName_is_null()
        {
            Action act = () => _ = new ErrorLogEntry(null, "content", "/path");

            act.Should().Throw<ArgumentNullException>().WithParameterName("fileName");
        }

        [Fact]
        public void Constructor_throws_when_content_is_null()
        {
            Action act = () => _ = new ErrorLogEntry("file.txt", null, "/path");

            act.Should().Throw<ArgumentNullException>().WithParameterName("content");
        }

        [Fact]
        public void Constructor_throws_when_filePath_is_null()
        {
            Action act = () => _ = new ErrorLogEntry("file.txt", "content", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("filePath");
        }
    }
}
