// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporation.Mods.Abstractions;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Interfaces;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.ImplementationTests
{
    public sealed class PresetWriterTests
    {
        private static readonly string[] s_twoFiles = { "Presets/a.json", "Presets/b.json" };

        [Fact]
        public void DeletePreset_writes_each_file_where_the_preset_was_found()
        {
            var mockLoader = new Mock<IPresetLoader>();
            mockLoader.Setup(l => l.FindAllPresetFiles()).Returns(s_twoFiles);
            mockLoader
                .Setup(l => l.LoadPresetsFromCustomFile("Presets/a.json"))
                .Returns(BuildListWith("Alice"));
            mockLoader
                .Setup(l => l.LoadPresetsFromCustomFile("Presets/b.json"))
                .Returns(BuildListWith("Alice"));
            var mockFileManager = BuildFileManager();

            var sut = new PresetWriter(
                mockFileManager.Object,
                mockLoader.Object,
                new Mock<IUiController>().Object
            );

            sut.DeletePreset("Alice");

            mockFileManager.Verify(
                fm => fm.WriteAllText("Presets/a.json", It.IsAny<string>()),
                Times.Once
            );
            mockFileManager.Verify(
                fm => fm.WriteAllText("Presets/b.json", It.IsAny<string>()),
                Times.Once
            );
        }

        [Fact]
        public void DeletePreset_skips_files_that_do_not_contain_the_preset()
        {
            var mockLoader = new Mock<IPresetLoader>();
            mockLoader.Setup(l => l.FindAllPresetFiles()).Returns(s_twoFiles);
            mockLoader
                .Setup(l => l.LoadPresetsFromCustomFile("Presets/a.json"))
                .Returns(BuildListWith("Alice"));
            mockLoader
                .Setup(l => l.LoadPresetsFromCustomFile("Presets/b.json"))
                .Returns(BuildListWith("Bob"));
            var mockFileManager = BuildFileManager();

            var sut = new PresetWriter(
                mockFileManager.Object,
                mockLoader.Object,
                new Mock<IUiController>().Object
            );

            sut.DeletePreset("Alice");

            mockFileManager.Verify(
                fm => fm.WriteAllText("Presets/a.json", It.IsAny<string>()),
                Times.Once
            );
            mockFileManager.Verify(
                fm => fm.WriteAllText("Presets/b.json", It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public void DeletePreset_is_a_noop_when_no_preset_files_exist()
        {
            var mockLoader = new Mock<IPresetLoader>();
            mockLoader.Setup(l => l.FindAllPresetFiles()).Returns(new List<string>());
            var mockFileManager = BuildFileManager();

            var sut = new PresetWriter(
                mockFileManager.Object,
                mockLoader.Object,
                new Mock<IUiController>().Object
            );

            sut.DeletePreset("Alice");

            mockFileManager.Verify(
                fm => fm.WriteAllText(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        private static PresetList BuildListWith(string presetName)
        {
            var list = new PresetList();
            list.Presets[presetName] = new PresetData();

            return list;
        }

        private static Mock<IFileManager> BuildFileManager()
        {
            var mockFileManager = new Mock<IFileManager>();
            mockFileManager
                .Setup(fm => fm.GetFile(It.IsAny<string>()))
                .Returns<string>(name => name);

            return mockFileManager;
        }
    }
}
