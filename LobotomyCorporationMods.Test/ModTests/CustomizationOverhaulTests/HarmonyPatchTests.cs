// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using Customizing;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.CustomizationOverhaul;
using LobotomyCorporationMods.CustomizationOverhaul.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests
{
    public sealed class HarmonyPatchTests
    {
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
            const string MethodName = "Awake";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_Awake_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            mockLogger.VerifyArgumentNullException(AgentInfoWindowPatchAwake.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CloseWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchCloseWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.CloseWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CloseWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            mockLogger.VerifyArgumentNullException(AgentInfoWindowPatchCloseWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CreateWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchCreateWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.CreateWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_CreateWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            mockLogger.VerifyArgumentNullException(AgentInfoWindowPatchCreateWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchEnforcementWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.EnforcementWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            mockLogger.VerifyArgumentNullException(AgentInfoWindowPatchEnforcementWindow.Postfix);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_GenerateWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchGenerateWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = nameof(AgentInfoWindow.GenerateWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_GenerateWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            mockLogger.VerifyArgumentNullException(AgentInfoWindowPatchGenerateWindow.Postfix);
        }

        [Fact]
        public void Class_AppearanceUI_Method_CloseWindow_is_patched_correctly()
        {
            var patch = typeof(AppearanceUiPatchCloseWindow);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.CloseWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_CloseWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                AppearanceUiPatchCloseWindow.Prefix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_AppearanceUI_Method_InitialDataLoad_is_patched_correctly()
        {
            var patch = typeof(AppearanceUiPatchInitialDataLoad);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.InitialDataLoad);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_InitialDataLoad_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                AppearanceUiPatchInitialDataLoad.Postfix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_AppearanceUI_Method_UpdatePortrait_is_patched_correctly()
        {
            var patch = typeof(AppearanceUIPatchUpdatePortrait);
            var originalClass = typeof(AppearanceUI);
            const string MethodName = nameof(AppearanceUI.UpdatePortrait);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AppearanceUI_Method_UpdatePortrait_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                AppearanceUIPatchUpdatePortrait.Postfix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Cancel_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchCancel);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.Cancel);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Cancel_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                CustomizingWindowPatchCancel.Postfix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Confirm_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchConfirm);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.Confirm);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_Confirm_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                CustomizingWindowPatchConfirm.Prefix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchOpenAppearanceWindow);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.OpenAppearanceWindow);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                CustomizingWindowPatchOpenAppearanceWindow.Postfix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchReviseOpenAction);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = GameMethods.CustomizingWindow.ReviseOpenAction;

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null arguments to test exception logging.
                CustomizingWindowPatchReviseOpenAction.Postfix(null, null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_WorkerSpriteManager_Method_LoadCustomSprites_is_patched_correctly()
        {
            var patch = typeof(WorkerSpriteManagerPatchLoadCustomSprites);
            var originalClass = typeof(WorkerSpriteManager);
            const string MethodName = nameof(WorkerSpriteManager.LoadCustomSprites);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_WorkerSpriteManager_Method_LoadCustomSprites_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.SetLogger(mockLogger.Object);

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                WorkerSpriteManagerPatchLoadCustomSprites.Postfix(null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }
    }
}
