// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.PatchTests
{
    public sealed class WorkerSpriteManagerPatchLoadCustomSpritesTests
        : CustomizationOverhaulModTests,
            IDisposable
    {
        private readonly string _tempCustomDataFolder = Path.Combine(
            Path.GetTempPath(),
            "customization-overhaul-test-" + Guid.NewGuid().ToString("N")
        );

        public void Dispose()
        {
            if (Directory.Exists(_tempCustomDataFolder))
            {
                Directory.Delete(_tempCustomDataFolder, recursive: true);
            }
        }

        [Fact]
        public void Returns_early_when_CustomData_folder_is_missing()
        {
            var sut = UnityTestExtensions.CreateWorkerSpriteManager();
            var mockFileManager = new Mock<IFileManager>();
            mockFileManager
                .Setup(fm => fm.GetFile("CustomData"))
                .Returns("/tmp/does-not-exist-customdata-folder");

            sut.PatchAfterLoadCustomSprites(mockFileManager.Object);

            mockFileManager.Verify(fm => fm.GetFile("CustomData"), Times.Once);
        }

        [Fact]
        public void Iterates_sprite_region_map_when_CustomData_folder_exists()
        {
            Directory.CreateDirectory(_tempCustomDataFolder);
            var sut = UnityTestExtensions.CreateWorkerSpriteManager();
            var mockFileManager = new Mock<IFileManager>();
            mockFileManager.Setup(fm => fm.GetFile("CustomData")).Returns(_tempCustomDataFolder);

            // Subdirectories for each sprite region are intentionally absent, so
            // LoadSpriteIfExists returns early without invoking the Unity-bound loader.
            sut.PatchAfterLoadCustomSprites(mockFileManager.Object);

            mockFileManager.Verify(fm => fm.GetFile("CustomData"), Times.Once);
        }
    }
}
