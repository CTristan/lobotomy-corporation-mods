// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using Customizing;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization.Patches
{
    public sealed class CustomizingWindowPatchConfirmTests : FreeCustomizationTests
    {
        private readonly Mock<IAgentLayerAdapter> _mockAgentLayerAdapter = new Mock<IAgentLayerAdapter>();

        private readonly Mock<IWorkerSpriteManagerAdapter> _mockWorkerSpriteManagerAdapter =
            new Mock<IWorkerSpriteManagerAdapter>();

        [Fact]
        public void Changing_random_generated_agent_marks_them_as_custom()
        {
            // Arrange
            var sut = InitializeCustomizingWindow(CustomizingType.REVISE);
            var agent = TestExtensions.CreateAgentModel();
            agent.iscustom = false;

            // Act
            sut.PatchBeforeConfirm(_mockAgentLayerAdapter.Object, _mockWorkerSpriteManagerAdapter.Object);

            // Assert
            sut.CurrentAgent.iscustom.Should().Be(true);
        }

        [Fact]
        public void Customizing_existing_agent_changes_agent_appearance_successfully()
        {
            // Arrange
            var currentAppearance = TestExtensions.CreateWorkerSprite();
            var expectedSprite = TestExtensions.CreateSprite();
            var expectedColor = Color.black;
            var expectedAppearance = new Appearance
            {
                spriteSet = TestExtensions.CreateWorkerSprite(),
                Eyebrow_Battle = expectedSprite,
                FrontHair = expectedSprite,
                RearHair = expectedSprite,
                Eyebrow_Def = expectedSprite,
                Eyebrow_Panic = expectedSprite,
                Eye_Battle = expectedSprite,
                Eye_Def = expectedSprite,
                Eye_Panic = expectedSprite,
                Eye_Dead = expectedSprite,
                Mouth_Def = expectedSprite,
                Mouth_Battle = expectedSprite,
                HairColor = expectedColor,
                EyeColor = expectedColor
            };

            var currentAgent = TestExtensions.CreateAgentModel();
            currentAgent.spriteData = currentAppearance;

            var agentData = TestExtensions.CreateAgentData();
            agentData.appearance = expectedAppearance;

            var sut = InitializeCustomizingWindow(CustomizingType.REVISE);
            sut.appearanceUI.copied = agentData;
            sut.CurrentData.appearance = expectedAppearance;

            // Act
            sut.PatchBeforeConfirm(_mockAgentLayerAdapter.Object, _mockWorkerSpriteManagerAdapter.Object);

            // Assert
            sut.CurrentAgent.spriteData.Should().BeEquivalentTo(expectedAppearance.spriteSet);
            sut.CurrentAgent.spriteData.BattleEyeBrow.GetHashCode().Should().Be(expectedSprite.GetHashCode());
            sut.CurrentAgent.spriteData.HairColor.Should().Be(expectedColor);
        }

        [Theory]
        [InlineData("CurrentName", "ExpectedName")]
        [InlineData("OldName", "NewName")]
        public void Renaming_agent_changes_agent_name_successfully([NotNull] string currentName, [NotNull] string expectedName)
        {
            // Arrange
            var currentAgent = TestExtensions.CreateAgentModel();
            currentAgent.name = currentName;
            currentAgent._agentName.nameDic = new Dictionary<string, string> { { currentName, currentName } };

            var expectedAgentName = TestExtensions.CreateAgentName();
            expectedAgentName.nameDic = new Dictionary<string, string> { { expectedName, expectedName } };

            var expectedData = TestExtensions.CreateAgentData();
            expectedData.CustomName = expectedName;
            expectedData.agentName = expectedAgentName;

            var sut = InitializeCustomizingWindow(currentAgent, CustomizingType.REVISE);
            sut.CurrentData = expectedData;

            // Act
            sut.PatchBeforeConfirm(_mockAgentLayerAdapter.Object, _mockWorkerSpriteManagerAdapter.Object);

            // Assert
            sut.CurrentAgent.name.Should().Be(expectedName);
            sut.CurrentAgent._agentName.metaInfo.nameDic.Should().ContainValue(expectedName);
            sut.CurrentAgent._agentName.nameDic.Should().ContainValue(expectedName);
        }

        [Fact]
        public void Does_not_attempt_to_update_newly_generated_agents()
        {
            // Arrange
            // Default for CustomizingWindow is Generate
            var sut = InitializeCustomizingWindow();

            // Act
            sut.PatchBeforeConfirm(_mockAgentLayerAdapter.Object, _mockWorkerSpriteManagerAdapter.Object);

            // Assert
            _mockWorkerSpriteManagerAdapter.Verify(
                x => x.SetAgentBasicData(It.IsAny<WorkerSprite.WorkerSprite>(), It.IsAny<Appearance>()), Times.Never);
        }
    }
}
