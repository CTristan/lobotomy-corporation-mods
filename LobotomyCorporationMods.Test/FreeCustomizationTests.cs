// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.FreeCustomization.Extensions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using Xunit;
using Xunit.Extensions;

#endregion

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    public sealed class FreeCustomizationTests
    {
        public FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        [Fact]
        public void Changing_random_generated_agent_marks_them_as_custom()
        {
            var agent = TestExtensions.CreateAgentModel();
            agent.iscustom = false;
            var customizingWindow = TestExtensions.CreateCustomizingWindow(currentAgent: agent);

            customizingWindow.RenameAgent();

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
            customizingWindow.SaveAgentAppearance();

            // Assert
            customizingWindow.CurrentAgent.spriteData.ShouldBeEquivalentTo(expectedAppearance.spriteSet);
            customizingWindow.CurrentAgent.spriteData.BattleEyeBrow.GetHashCode().ShouldBeEquivalentTo(expectedSprite.GetHashCode());
            customizingWindow.CurrentAgent.spriteData.HairColor.Should().Be(expectedColor);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var customizingWindow = GetCustomizingWindow();
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = true };

            CustomizingWindowPatchOpenAppearanceWindow.Postfix(customizingWindow);

            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }

        [Theory]
        [InlineData("DefaultAgent")]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_gets_agent_appearance_data([NotNull] string agentName)
        {
            var customizingWindow = GetCustomizingWindow();
            var agentModel = TestExtensions.CreateAgentModel();
            agentModel.name = agentName;

            CustomizingWindowPatchReviseOpenAction.Postfix(customizingWindow, agentModel);

            customizingWindow.CurrentData.CustomName.Should().Be(agentName);
        }

        [Fact]
        public void Opening_the_strengthen_employee_window_opens_the_Appearance_UI()
        {
            var agentInfoWindow = TestExtensions.CreateAgentInfoWindow();
            var mockAgentInfoWindowAdapter = new Mock<IAgentInfoWindowAdapter>();

            agentInfoWindow.OpenAppearanceWindow(mockAgentInfoWindowAdapter.Object);

            mockAgentInfoWindowAdapter.Verify(x => x.OpenAppearanceWindow(agentInfoWindow), Times.Once);
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
            customizingWindow.RenameAgent();

            // Assert
            customizingWindow.CurrentAgent.name.Should().Be(expectedName);
            customizingWindow.CurrentAgent._agentName.metaInfo.nameDic.Should().ContainValue(expectedName);
            customizingWindow.CurrentAgent._agentName.nameDic.Should().ContainValue(expectedName);
        }

        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = new AppearanceUI();

            var result = AppearanceUIPatchCloseWindow.Prefix(appearanceUi);

            result.Should().BeFalse();
        }

        #region Code Coverage Tests

        [Fact]
        public void AgentInfoWindowPatchEnforcementWindow_Is_Untestable()
        {
            // Requires an AgentInfoWindow instance
            TestExtensions.CreateAgentInfoWindow();

            Action action = AgentInfoWindowPatchEnforcementWindow.Postfix;

            action.ShouldThrowUnityException();
        }

        [Fact]
        public void CustomizingWindowPatchConfirm_Is_Untestable()
        {
            var customizingWindow = GetCustomizingWindow();

            Action action = () => CustomizingWindowPatchConfirm.Prefix(customizingWindow);

            action.ShouldThrowUnityException();
        }

        #endregion

        #region Harmony Tests

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.ShouldNotThrow();
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchEnforcementWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = "EnforcementWindow";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_GenerateWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchGenerateWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = "GenerateWindow";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_CloseWindow_is_patched_correctly()
        {
            var patch = typeof(AppearanceUIPatchCloseWindow);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = "CloseWindow";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Confirm_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchConfirm);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "Confirm";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchOpenAppearanceWindow);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "OpenAppearanceWindow";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchReviseOpenAction);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "ReviseOpenAction";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        #endregion

        #region Helper Methods

        [NotNull]
        private static CustomizingWindow GetCustomizingWindow()
        {
            // Need a WorkerSpriteManager instance
            InitializeWorkerSpriteManager();

            return TestExtensions.CreateCustomizingWindow();
        }

        private static void InitializeWorkerSpriteManager()
        {
            TestExtensions.CreateWorkerSpriteManager();
        }

        #endregion
    }
}
