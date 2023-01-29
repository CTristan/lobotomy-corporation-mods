// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Customizing;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.FreeCustomization.Extensions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using NSubstitute;
using UnityEngine;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    public sealed class FreeCustomizationTests
    {
        public FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var fileManager = TestExtensions.CreateFileManager();
            Harmony_Patch.Instance.LoadData(fileManager);
        }

        [Fact]
        public void The_Appearance_UI_does_not_close_itself_if_there_is_no_close_action()
        {
            var appearanceUi = Substitute.For<AppearanceUI>();

            var result = AppearanceUIPatchCloseWindow.Prefix(appearanceUi);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Opening_the_customize_appearance_window_does_not_increase_the_cost_of_hiring_the_agent(bool isCustomAppearance)
        {
            var customizingWindow = TestData.DefaultCustomizingWindow;
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = true };

            CustomizingWindowPatchOpenAppearanceWindow.Postfix(customizingWindow);

            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }

        [Theory]
        [InlineData(TestData.DefaultAgentName)]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_gets_agent_appearance_data([NotNull] string agentName)
        {
            var customizingWindow = TestData.DefaultCustomizingWindow;
            var agentModel = TestData.DefaultAgentModel;
            agentModel.name = agentName;

            CustomizingWindowPatchReviseOpenAction.Postfix(customizingWindow, agentModel);

            customizingWindow.CurrentData.CustomName.Should().Be(agentName);
        }

        [Theory]
        [InlineData("Current", "Expected")]
        [InlineData("Old", "New")]
        public void Recustomizing_agent_changes_agent_appearance_successfully(string currentAppearanceName, string expectedAppearanceName)
        {
            // Arrange
            var currentAppearance = TestData.DefaultWorkerSprite;
            var expectedSprite = TestData.DefaultSprite;
            var expectedColor = Color.black;
            var expectedAppearance = new Appearance
            {
                spriteSet = TestData.DefaultWorkerSprite,
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

            var currentAgent = TestData.DefaultAgentModel;
            currentAgent.spriteData = currentAppearance;

            var customizingWindow = TestExtensions.CreateCustomizingWindow(TestData.DefaultAppearanceUI, TestData.DefaultAgentModel, TestData.DefaultAgentData, CustomizingType.REVISE);
            customizingWindow.CurrentData.appearance = expectedAppearance;

            // Act
            customizingWindow.SaveAgentAppearance();

            // Assert
            customizingWindow.CurrentAgent.spriteData.ShouldBeEquivalentTo(expectedAppearance.spriteSet);
            customizingWindow.CurrentAgent.spriteData.BattleEyeBrow.GetHashCode().ShouldBeEquivalentTo(expectedSprite.GetHashCode());
        }

        #region Code Coverage Tests

        [Fact]
        public void CustomizingWindowPatchReviseOpenAction_Is_Untestable()
        {
            Action action = () => CustomizingWindowPatchReviseOpenAction.Postfix(new CustomizingWindow(), new AgentModel(TestData.DefaultAgentId));

            action.ShouldThrow<SecurityException>();
        }

        [Fact]
        public void CustomizingWindowPatchConfirm_Is_Untestable()
        {
            // WorkerSpriteManager needed for patch
            TestExtensions.CreateWorkerSpriteManager(TestData.DefaultWorkerBasicSpriteController);
            var customizingWindow = TestExtensions.CreateCustomizingWindow(TestData.DefaultAppearanceUI, TestData.DefaultAgentModel, TestData.DefaultAgentData, CustomizingType.REVISE);

            Action action = () => CustomizingWindowPatchConfirm.Prefix(customizingWindow);

            // Gets a null reference trying to access the WorkerManager instance
            action.ShouldThrow<SecurityException>();
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
    }
}
