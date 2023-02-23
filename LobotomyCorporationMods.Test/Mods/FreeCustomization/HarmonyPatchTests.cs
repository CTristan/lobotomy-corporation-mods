// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.FreeCustomization.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_is_patched_correctly()
        {
            var patch = typeof(AgentInfoWindowPatchEnforcementWindow);
            var originalClass = typeof(AgentInfoWindow);
            const string MethodName = "EnforcementWindow";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_AgentInfoWindow_Method_EnforcementWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => AgentInfoWindowPatchEnforcementWindow.Postfix();

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
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
        public void Class_AgentInfoWindow_Method_GenerateWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => AgentInfoWindowPatchGenerateWindow.Postfix();

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
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
        public void Class_AppearanceUI_Method_CloseWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => AppearanceUIPatchCloseWindow.Prefix(null!);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
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
        public void Class_CustomizingWindow_Method_Confirm_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => CustomizingWindowPatchConfirm.Postfix(null!);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
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
        public void Class_CustomizingWindow_Method_OpenAppearanceWindow_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => CustomizingWindowPatchOpenAppearanceWindow.Postfix(null!);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchReviseOpenAction);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "ReviseOpenAction";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_ReviseOpenAction_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => CustomizingWindowPatchReviseOpenAction.Postfix(null!, null!);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);

            // Verify other arguments throw an exception if null
            action = static () => CustomizingWindowPatchReviseOpenAction.Postfix(TestExtensions.CreateCustomizingWindow(), null!);
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () => _ = new Harmony_Patch();
            action.ShouldNotThrow();
        }
    }
}
