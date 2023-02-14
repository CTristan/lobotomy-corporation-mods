// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.BugFixesTests
{
    public sealed class BugFixesTests
    {
        public BugFixesTests()
        {
            _ = new Harmony_Patch();
            var logger = new Mock<ILogger>();
            Harmony_Patch.Instance.LoadData(logger.Object);
        }

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
        public void Class_ArmorCreature_Method_OnNotice_is_patched_correctly()
        {
            var patch = typeof(ArmorCreaturePatchOnNotice);
            var originalClass = typeof(ArmorCreature);
            const string MethodName = "OnNotice";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = "SetAgentStatBonus";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => CustomizingWindowPatchSetAgentStatBonus.Prefix(null, null, null);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_returns_false()
        {
            // Arrange
            var customizingWindow = TestExtensions.CreateCustomizingWindow();
            var agentModel = TestExtensions.CreateAgentModel();
            var agentData = TestExtensions.CreateAgentData();

            var mockCustomizingWindowAdapter = new Mock<ICustomizingWindowAdapter>();
            CustomizingWindowPatchSetAgentStatBonus.CustomizingWindowAdapter = mockCustomizingWindowAdapter.Object;

            // Act
            var result = CustomizingWindowPatchSetAgentStatBonus.Prefix(customizingWindow, agentModel, agentData);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
