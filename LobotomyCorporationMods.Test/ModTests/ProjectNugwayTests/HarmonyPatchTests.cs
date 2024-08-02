// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.ProjectNugway;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests
{
    public sealed class HarmonyPatchTests : HarmonyPatchTestBase
    {
        public HarmonyPatchTests()
        {
            Harmony_Patch.Instance.AddLoggerTarget(MockLogger.Object);
        }

        /// <summary>Harmony requires the constructor to be public.</summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () =>
            {
                _ = new Harmony_Patch();
            };

            action.Should().NotThrow();
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_Awake_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchAwake);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(PrivateMethods.AgentInfoWindow.Awake);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_Awake_logs_exceptions()
        {
            VerifyArgumentNullExceptionLogging(AgentInfoWindowPatchAwake.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CloseWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchCloseWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.CloseWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CloseWindow_logs_exceptions()
        {
            VerifyArgumentNullExceptionLogging(AgentInfoWindowPatchCloseWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CreateWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchCreateWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.CreateWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CreateWindow_logs_exceptions()
        {
            VerifyArgumentNullExceptionLogging(AgentInfoWindowPatchCreateWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchEnforcementWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.EnforcementWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_logs_exceptions()
        {
            VerifyArgumentNullExceptionLogging(AgentInfoWindowPatchEnforcementWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_GenerateWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchGenerateWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.GenerateWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_GenerateWindow_logs_exceptions()
        {
            VerifyArgumentNullExceptionLogging(AgentInfoWindowPatchGenerateWindow.Postfix);
        }

        [Fact]
        public void Class_AppearanceUI_Method_CloseWindow_is_patched_correctly()
        {
            var patch = typeof(AppearanceUiPatchCloseWindow);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.CloseWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_CloseWindow_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => AppearanceUiPatchCloseWindow.Prefix(null));
        }

        [Fact]
        public void Class_AppearanceUI_Method_InitialDataLoad_is_patched_correctly()
        {
            var patch = typeof(AppearanceUiPatchInitialDataLoad);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.InitialDataLoad);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_InitialDataLoad_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => AppearanceUiPatchInitialDataLoad.Postfix(null));
        }

        [Fact]
        public void Class_AppearanceUI_Method_UpdatePortrait_is_patched_correctly()
        {
            var patch = typeof(AppearanceUIPatchUpdatePortrait);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.UpdatePortrait);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_UpdatePortrait_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => AppearanceUIPatchUpdatePortrait.Postfix(null));
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Cancel_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchCancel);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.Cancel);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Cancel_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchCancel.Postfix(null));
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Confirm_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchConfirm);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.Confirm);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Confirm_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchConfirm.Prefix(null));
        }

        [Fact]
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchOpenAppearanceWindow);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.OpenAppearanceWindow);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchOpenAppearanceWindow.Postfix(null));
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchReviseOpenAction);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = PrivateMethods.CustomizingWindow.ReviseOpenAction;

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_logs_exceptions()
        {
            var times = 1;

            // ReSharper disable AssignNullToNotNullAttribute
            // Forcing null arguments to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchReviseOpenAction.Postfix(null, null), Times.Exactly(times++));
            // ReSharper enable AssignNullToNotNullAttribute

            // Verify other arguments throw an exception if null
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchReviseOpenAction.Postfix(UnityTestExtensions.CreateCustomizingWindow(), null), Times.Exactly(times));
        }
    }
}
