// SPDX-License-Identifier:MIT

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
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.FreeCustomizationTests.Patches
{
    public sealed class CustomizingWindowPatchConfirmTests : FreeCustomizationTests
    {
        public CustomizingWindowPatchConfirmTests()
        {
            CustomizingWindowPatchConfirm.SpriteManagerAdapter = new Mock<IWorkerSpriteManagerAdapter>().Object;
            CustomizingWindowPatchConfirm.LayerAdapter = new Mock<IAgentLayerAdapter>().Object;
        }

        [Fact]
        public void Changing_random_generated_agent_marks_them_as_custom()
        {
            // Arrange
            var agent = TestExtensions.CreateAgentModel();
            agent.iscustom = false;

            var customizingWindow = GetCustomizingWindow();

            // Act
            CustomizingWindowPatchConfirm.Prefix(customizingWindow);

            // Assert
            customizingWindow.CurrentAgent.iscustom.Should().Be(true);
        }

        [Theory]
        [InlineData("Current", "Expected")]
        [InlineData("Old", "New")]
        public void Customizing_existing_agent_changes_agent_appearance_successfully(string currentAppearanceName, string expectedAppearanceName)
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

            var customizingWindow = GetCustomizingWindow();
            customizingWindow.CurrentData.appearance = expectedAppearance;

            // Act
            CustomizingWindowPatchConfirm.Prefix(customizingWindow);

            // Assert
            customizingWindow.CurrentAgent.spriteData.ShouldBeEquivalentTo(expectedAppearance.spriteSet);
            customizingWindow.CurrentAgent.spriteData.BattleEyeBrow.GetHashCode().ShouldBeEquivalentTo(expectedSprite.GetHashCode());
            customizingWindow.CurrentAgent.spriteData.HairColor.Should().Be(expectedColor);
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

            var customizingWindow = TestExtensions.CreateCustomizingWindow(currentAgent: currentAgent);
            customizingWindow.CurrentData = expectedData;

            // Act
            CustomizingWindowPatchConfirm.Prefix(customizingWindow);

            // Assert
            customizingWindow.CurrentAgent.name.Should().Be(expectedName);
            customizingWindow.CurrentAgent._agentName.metaInfo.nameDic.Should().ContainValue(expectedName);
            customizingWindow.CurrentAgent._agentName.nameDic.Should().ContainValue(expectedName);
        }
    }
}
