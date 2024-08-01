// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests
{
    public sealed class HarmonyPatchTests : HarmonyPatchTestBase
    {
        public HarmonyPatchTests()
        {
            Harmony_Patch.Instance.AddLoggerTarget(MockLogger.Object);
        }

        /// <summary>Harmony requires the constructor to be public.</summary>
        [Fact]
        public void BadLuckProtectionForGifts_Constructor_is_public_and_externally_accessible()
        {
            Action act = () =>
            {
                _ = new Harmony_Patch();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_is_patched_correctly()
        {
            var patch = typeof(ArmorCreaturePatchOnNotice);
            var originalClass = typeof(ArmorCreature);
            const string MethodName = nameof(ArmorCreature.OnNotice);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_logs_exceptions()
        {
            var armorCreature = new ArmorCreature();

            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => ArmorCreaturePatchOnNotice.Postfix(armorCreature, string.Empty, null));
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_is_patched_correctly()
        {
            var patch = typeof(CustomizingWindowPatchSetAgentStatBonus);
            var originalClass = typeof(CustomizingWindow);
            const string MethodName = nameof(CustomizingWindow.SetAgentStatBonus);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_logs_exceptions()
        {
            var times = 1;

            // ReSharper disable AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchSetAgentStatBonus.Prefix(null, null, null), Times.Exactly(times++));
            // ReSharper enable AssignNullToNotNullAttribute

            // Verify other arguments throw exception when null
            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchSetAgentStatBonus.Prefix(UnityTestExtensions.CreateCustomizingWindow(), null, UnityTestExtensions.CreateAgentData()),
                Times.Exactly(times++));

            VerifyArgumentNullExceptionLogging(() => CustomizingWindowPatchSetAgentStatBonus.Prefix(UnityTestExtensions.CreateCustomizingWindow(), UnityTestExtensions.CreateAgentModel(), null),
                Times.Exactly(times));
        }
    }
}
