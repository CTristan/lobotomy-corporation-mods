// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Customizing;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.FreeCustomization.Extensions;
using LobotomyCorporationMods.FreeCustomization.Patches;
using NSubstitute;
using Xunit;
using Xunit.Extensions;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    public sealed class FreeCustomizationTests
    {
        private const long DefaultAgentId = 1L;
        private const string DefaultAgentName = "DefaultAgentName";

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
            var customizingWindow = Substitute.For<CustomizingWindow>();
            customizingWindow.CurrentData = new AgentData { isCustomAppearance = true };

            CustomizingWindowPatchOpenAppearanceWindow.Postfix(customizingWindow);

            customizingWindow.CurrentData.isCustomAppearance.Should().BeFalse();
        }

        [Theory]
        [InlineData(DefaultAgentName)]
        [InlineData("TestAgent")]
        public void Opening_the_strengthen_employee_window_sets_appearance_data([NotNull] string agentName)
        {
            var customizingWindow = Substitute.For<CustomizingWindow>();
            customizingWindow.CurrentData = new AgentData();
            var agentModel = GetAgentModel(agentName);
            agentModel._agentName = new AgentName(1) { nameDic = new Dictionary<string, string> { { agentName, agentName } } };
            agentModel.spriteData = new WorkerSprite.WorkerSprite();
            var agentInfoWindow = Substitute.For<AgentInfoWindow>();
            agentInfoWindow.UIComponents = Substitute.For<AgentInfoWindow.UIComponent>();

            customizingWindow.SetAppearanceData(agentModel, agentInfoWindow);

            customizingWindow.CurrentData.CustomName.Should().Be(agentName);
        }

        #region Helper Methods

        [NotNull]
        private static AgentModel GetAgentModel(string agentName)
        {
            return TestExtensions.CreateAgentModel(DefaultAgentId, agentName);
        }

        #endregion

        #region Code Coverage Tests

        [Fact]
        public void CustomizingWindowPatchReviseOpenAction_Is_Untestable()
        {
            Action action = () => CustomizingWindowPatchReviseOpenAction.Postfix(new CustomizingWindow(), new AgentModel(DefaultAgentId));

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
