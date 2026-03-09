// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using Customizing;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests
{
    public sealed class HarmonyPatchTests
    {
        /// <summary>Harmony requires the constructor to be public.</summary>
        [Fact]
        public void BadLuckProtectionForGifts_Constructor_is_public_and_externally_accessible()
        {
            Action act = () =>
            {
                _ = new Harmony_Patch();
            };

            _ = act.Should().NotThrow();
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_is_patched_correctly()
        {
            Type patch = typeof(ArmorCreaturePatchOnNotice);
            Type originalClass = typeof(ArmorCreature);
            const string MethodName = nameof(ArmorCreature.OnNotice);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_logs_exceptions()
        {
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
            ArmorCreature armorCreature = new();

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                ArmorCreaturePatchOnNotice.Postfix(armorCreature, string.Empty, null);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly()
        {
            Type patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            Type originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.SetAgentStatBonus);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_logs_exceptions()
        {
            Mock<ILogger> mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
            const int TwoLogs = 2;
            const int ThreeLogs = 3;

            // ReSharper disable AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            Action action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(null, null, null);
            // ReSharper enable AssignNullToNotNullAttribute

            mockLogger.VerifyArgumentNullException(action);

            // Verify other arguments throw exception when null
            action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(UnityTestExtensions.CreateCustomizingWindow(), null, UnityTestExtensions.CreateAgentData());
            mockLogger.VerifyArgumentNullException(action, Times.Exactly(TwoLogs));
            action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(UnityTestExtensions.CreateCustomizingWindow(), UnityTestExtensions.CreateAgentModel(), null);
            mockLogger.VerifyArgumentNullException(action, Times.Exactly(ThreeLogs));
        }
    }
}
