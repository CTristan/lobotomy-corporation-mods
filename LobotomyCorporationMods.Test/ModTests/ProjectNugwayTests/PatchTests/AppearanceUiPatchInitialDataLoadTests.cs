// SPDX-License-Identifier: MIT

#region

using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using WorkerSprite;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AppearanceUiPatchInitialDataLoadTests : ProjectNugwayModTests
    {
        private readonly Mock<AppearanceUI> _sut = new Mock<AppearanceUI>();

        [Fact]
        public void Panic_mouth_sprites_are_loaded_into_the_list_of_battle_mouth_sprites_after_the_initial_data_load()
        {
            _sut.Object.mouth_Battle = new Mock<SpriteSelector>().Object;

            // Set up the basic data with a panic mouth sprite.
            var spriteLoadData = new SpriteLoadData();
            var panicMouthSpriteData = new WorkerBasicSprite();
            panicMouthSpriteData.loadedData.Add(spriteLoadData);
            var basicData = new WorkerBasicSpriteController();
            basicData.lib.Add(BasicSpriteRegion.MOUTH_PANIC, panicMouthSpriteData);
            UnityTestExtensions.CreateWorkerSpriteManager(basicData);

            _sut.Object.PatchAfterInitialDataLoad();

            _sut.Object.mouth_Battle.SpriteList.Should().Contain(spriteLoadData.sprite);
        }
    }
}
