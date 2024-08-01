// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BadLuckProtectionForGiftsTests
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
        public void Class_CreatureEquipmentMakeInfo_Method_GetProb_is_patched_correctly()
        {
            var patch = typeof(CreatureEquipmentMakeInfoPatchGetProb);
            var originalClass = typeof(CreatureEquipmentMakeInfo);
            const string MethodName = nameof(CreatureEquipmentMakeInfo.GetProb);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_CreatureEquipmentMakeInfo_Method_GetProb_logs_exceptions()
        {
            var result = 0f;

            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => CreatureEquipmentMakeInfoPatchGetProb.Postfix(null, ref result));
        }

        [Fact]
        public void Class_GameSceneController_Method_OnClickNextDay_is_patched_correctly()
        {
            var patch = typeof(GameSceneControllerPatchOnClickNextDay);
            var originalClass = typeof(GameSceneController);
            const string MethodName = nameof(GameSceneController.OnClickNextDay);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnClickNextDay_logs_exceptions()
        {
            VerifyNullReferenceExceptionLogging(GameSceneControllerPatchOnClickNextDay.Postfix);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnStageStart_is_patched_correctly()
        {
            var patch = typeof(GameSceneControllerPatchOnStageStart);
            var originalClass = typeof(GameSceneController);
            const string MethodName = nameof(GameSceneController.OnStageStart);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnStageStart_logs_exceptions()
        {
            VerifyNullReferenceExceptionLogging(GameSceneControllerPatchOnStageStart.Postfix);
        }

        [Fact]
        public void Class_NewTitleScript_Method_OnClickNewGame_is_patched_correctly()
        {
            var patch = typeof(NewTitleScriptPatchOnClickNewGame);
            var originalClass = typeof(NewTitleScript);
            const string MethodName = nameof(NewTitleScript.OnClickNewGame);

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_NewTitleScript_Method_OnClickNewGame_logs_exceptions()
        {
            VerifyNullReferenceExceptionLogging(NewTitleScriptPatchOnClickNewGame.Postfix);
        }

        [Fact]
        public void Class_UseSkill_Method_FinishWorkSuccessfully_is_patched_correctly()
        {
            var patch = typeof(UseSkillPatchFinishWorkSuccessfully);
            var originalClass = typeof(UseSkill);
            const string MethodName = PrivateMethods.UseSkill.FinishWorkSuccessfully;

            ValidatePatch(patch, originalClass, MethodName);
        }

        [Fact]
        public void Class_UseSkill_Method_FinishWorkSuccessfully_logs_exceptions()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            // Forcing null argument to test exception logging.
            VerifyArgumentNullExceptionLogging(() => UseSkillPatchFinishWorkSuccessfully.Postfix(null));
        }
    }
}
