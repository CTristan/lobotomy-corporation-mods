// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using AwesomeAssertions;
using Customizing;
using LobotomyCorporation.Mods.Abstractions;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Implementations;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using Moq;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.ImplementationTests
{
    public sealed class PresetLoaderTests
    {
        private const string PresetsDirectory = "Presets";
        private const string CustomFileName = "Presets/presets.json";
        private const string JsonFileMask = "*.json";
        private static readonly string[] s_noFiles = Array.Empty<string>();
        private static readonly string[] s_twoFiles = { "Presets/a.json", "Presets/b.json" };

        [Fact]
        public void HasPreset_returns_false_when_preset_is_not_loaded()
        {
            var sut = new PresetLoader(BuildFileManager().Object);

            sut.HasPreset("Alice").Should().BeFalse();
        }

        [Fact]
        public void HasPreset_returns_true_once_preset_is_added()
        {
            var sut = new PresetLoader(BuildFileManager().Object);
            sut.Presets["Alice"] = new PresetData();

            sut.HasPreset("Alice").Should().BeTrue();
        }

        [Fact]
        public void FindAllPresetFiles_creates_directory_when_missing()
        {
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.Exists(PresetsDirectory)).Returns(false);
            mockDirectory
                .Setup(d =>
                    d.GetFiles(PresetsDirectory, JsonFileMask, SearchOption.TopDirectoryOnly)
                )
                .Returns(s_noFiles);
            var mockFileManager = BuildFileManager(mockDirectory);

            var sut = new PresetLoader(mockFileManager.Object);

            sut.FindAllPresetFiles();

            mockDirectory.Verify(d => d.Create(PresetsDirectory), Times.Once);
        }

        [Fact]
        public void FindAllPresetFiles_returns_all_matching_files()
        {
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.Exists(PresetsDirectory)).Returns(true);
            mockDirectory
                .Setup(d =>
                    d.GetFiles(PresetsDirectory, JsonFileMask, SearchOption.TopDirectoryOnly)
                )
                .Returns(s_twoFiles);
            var mockFileManager = BuildFileManager(mockDirectory);

            var sut = new PresetLoader(mockFileManager.Object);

            sut.FindAllPresetFiles().Should().BeEquivalentTo("Presets/a.json", "Presets/b.json");
            mockDirectory.Verify(d => d.Create(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void InitializeDefaultCustomPresetFile_populates_presets_from_the_default_file()
        {
            var mockFileManager = BuildFileManager();
            mockFileManager
                .Setup(fm => fm.ReadAllText(CustomFileName, It.IsAny<bool>()))
                .Returns(BuildJson("Alice", "eyeBattle-value"));

            var sut = new PresetLoader(mockFileManager.Object);

            sut.InitializeDefaultCustomPresetFile();

            sut.Presets.Should().ContainKey("Alice");
            sut.Presets["Alice"].EyeBattle.Should().Be("eyeBattle-value");
        }

        [Fact]
        public void InitializeDefaultCustomPresetFile_leaves_presets_empty_when_file_is_empty()
        {
            var mockFileManager = BuildFileManager();
            mockFileManager
                .Setup(fm => fm.ReadAllText(CustomFileName, It.IsAny<bool>()))
                .Returns(string.Empty);

            var sut = new PresetLoader(mockFileManager.Object);

            sut.InitializeDefaultCustomPresetFile();

            sut.Presets.Should().BeEmpty();
        }

        [Fact]
        public void InitializeDefaultCustomPresetFile_leaves_presets_empty_when_json_is_malformed()
        {
            var mockFileManager = BuildFileManager();
            mockFileManager
                .Setup(fm => fm.ReadAllText(CustomFileName, It.IsAny<bool>()))
                .Returns("not-json");

            var sut = new PresetLoader(mockFileManager.Object);

            sut.InitializeDefaultCustomPresetFile();

            sut.Presets.Should().BeEmpty();
        }

        [Fact]
        public void ReloadPresetsFromFiles_clears_existing_presets_before_reloading()
        {
            var mockDirectory = new Mock<IDirectory>();
            mockDirectory.Setup(d => d.Exists(PresetsDirectory)).Returns(true);
            mockDirectory
                .Setup(d =>
                    d.GetFiles(PresetsDirectory, JsonFileMask, SearchOption.TopDirectoryOnly)
                )
                .Returns(s_noFiles);
            var mockFileManager = BuildFileManager(mockDirectory);
            mockFileManager
                .Setup(fm => fm.ReadAllText(CustomFileName, It.IsAny<bool>()))
                .Returns(string.Empty);

            var sut = new PresetLoader(mockFileManager.Object);
            sut.Presets["Stale"] = new PresetData();

            sut.ReloadPresetsFromFiles();

            sut.Presets.Should().NotContainKey("Stale");
        }

        [Fact]
        public void LoadPresetsFromCustomFile_reads_from_the_default_file_when_no_name_is_given()
        {
            var mockFileManager = BuildFileManager();
            mockFileManager
                .Setup(fm => fm.ReadAllText(CustomFileName, It.IsAny<bool>()))
                .Returns(BuildJson("Bob", "mouth-value"));

            var sut = new PresetLoader(mockFileManager.Object);

            var list = sut.LoadPresetsFromCustomFile();

            list.Presets.Should().ContainKey("Bob");
            list.Presets["Bob"].MouthBattle.Should().Be("mouth-value");
        }

        [Fact]
        public void LoadPresetsFromCustomFile_reads_from_the_named_file_when_given_one()
        {
            var mockFileManager = BuildFileManager();
            mockFileManager
                .Setup(fm => fm.ReadAllText("Presets/custom.json", It.IsAny<bool>()))
                .Returns(BuildJson("Carol", "hair-value"));

            var sut = new PresetLoader(mockFileManager.Object);

            var list = sut.LoadPresetsFromCustomFile("Presets/custom.json");

            list.Presets.Should().ContainKey("Carol");
            list.Presets["Carol"].FrontHair.Should().Be("hair-value");
        }

        [Fact]
        public void IsExactPreset_returns_false_when_no_preset_exists_for_that_agent()
        {
            var sut = new PresetLoader(BuildFileManager().Object);
            var appearance = new Appearance
            {
                spriteSet = new WorkerSprite.WorkerSprite(),
                HairColor = Color.white,
                EyeColor = Color.white,
            };

            sut.IsExactPreset("NoSuchAgent", appearance).Should().BeFalse();
        }

        [Fact]
        public void IsExactPreset_returns_true_when_colors_match_and_sprites_are_null()
        {
            var sut = new PresetLoader(BuildFileManager().Object);
            sut.Presets["Alice"] = new PresetData
            {
                HairColor = ColorUtility.ToHtmlStringRGB(Color.red),
                EyeColor = ColorUtility.ToHtmlStringRGB(Color.blue),
            };
            var appearance = new Appearance
            {
                spriteSet = new WorkerSprite.WorkerSprite(),
                HairColor = Color.red,
                EyeColor = Color.blue,
            };

            sut.IsExactPreset("Alice", appearance).Should().BeTrue();
        }

        [Fact]
        public void IsExactPreset_returns_false_when_hair_color_differs()
        {
            var sut = new PresetLoader(BuildFileManager().Object);
            sut.Presets["Alice"] = new PresetData
            {
                HairColor = ColorUtility.ToHtmlStringRGB(Color.red),
                EyeColor = ColorUtility.ToHtmlStringRGB(Color.blue),
            };
            var appearance = new Appearance
            {
                spriteSet = new WorkerSprite.WorkerSprite(),
                HairColor = Color.green,
                EyeColor = Color.blue,
            };

            sut.IsExactPreset("Alice", appearance).Should().BeFalse();
        }

        private static Mock<IFileManager> BuildFileManager(Mock<IDirectory> mockDirectory = null)
        {
            mockDirectory = mockDirectory ?? new Mock<IDirectory>();

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.SetupGet(fs => fs.Directory).Returns(mockDirectory.Object);

            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.SetupGet(fm => fm.FileSystem).Returns(mockFileSystem.Object);
            mockFileManager
                .Setup(fm => fm.GetFile(It.IsAny<string>()))
                .Returns<string>(name => name);
            mockFileManager
                .Setup(fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(string.Empty);

            return mockFileManager;
        }

        private static string BuildJson(string presetName, string value)
        {
            return "{\n  \""
                + presetName
                + "\": {\n"
                + $"    \"{nameof(PresetData.EyeBattle)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyeDead)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyeDef)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyePanic)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyebrowBattle)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyebrowDef)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyebrowPanic)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.FrontHair)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.MouthBattle)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.MouthDef)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.MouthPanic)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.RearHair)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.HairColor)}\": \"{value}\",\n"
                + $"    \"{nameof(PresetData.EyeColor)}\": \"{value}\"\n"
                + "  }\n}";
        }
    }
}
