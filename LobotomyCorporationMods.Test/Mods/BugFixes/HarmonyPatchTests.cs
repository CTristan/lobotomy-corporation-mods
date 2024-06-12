// SPDX-License-Identifier: MIT

#region

using System;
using Customizing;
using FluentAssertions;
using LobotomyCorporationMods.BugFixes;
using LobotomyCorporationMods.BugFixes.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BugFixes
{
    public sealed class HarmonyPatchTests
    {
        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
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
            const string MethodName = "OnNotice";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_ArmorCreature_Method_OnNotice_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            void Action()
            {
                ArmorCreaturePatchOnNotice.Prefix(string.Empty, null);
            }

            mockLogger.VerifyExceptionLogged<ArgumentNullException>(Action);
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
        public void Class_CustomizingWindow_Method_SetAgentStatBonus_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(null, null, null);

            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action);

            // Verify other arguments throw exception when null
            action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(TestExtensions.CreateCustomizingWindow(),
                null, TestExtensions.CreateAgentData());
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action, 2);
            action = () => CustomizingWindowPatchSetAgentStatBonus.Prefix(TestExtensions.CreateCustomizingWindow(),
                TestExtensions.CreateAgentModel(), null);
            mockLogger.VerifyExceptionLogged<ArgumentNullException>(action, 3);
        }
    }
}
