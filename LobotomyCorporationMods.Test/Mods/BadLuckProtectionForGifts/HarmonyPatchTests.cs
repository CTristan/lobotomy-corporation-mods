// SPDX-License-Identifier: MIT

#region

using System;
using FluentAssertions;
using LobotomyCorporationMods.BadLuckProtectionForGifts;
using LobotomyCorporationMods.BadLuckProtectionForGifts.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.BadLuckProtectionForGifts
{
    public sealed class HarmonyPatchTests
    {
        /// <summary>
        ///     Harmony requires the constructor to be public.
        /// </summary>
        [Fact]
        public void BadLuckProtectionForGifts_Constructor_is_public_and_externally_accessible()
        {
            Action act = () => _ = new Harmony_Patch();
            act.ShouldNotThrow();
        }

        [Fact]
        public void Class_CreatureEquipmentMakeInfo_Method_GetProb_is_patched_correctly()
        {
            var patch = typeof(CreatureEquipmentMakeInfoPatchGetProb);
            var originalClass = typeof(CreatureEquipmentMakeInfo);
            const string MethodName = nameof(CreatureEquipmentMakeInfo.GetProb);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_CreatureEquipmentMakeInfo_Method_GetProb_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
            var result = 0f;

            Action action = () => CreatureEquipmentMakeInfoPatchGetProb.Postfix(null, ref result);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnClickNextDay_is_patched_correctly()
        {
            var patch = typeof(GameSceneControllerPatchOnClickNextDay);
            var originalClass = typeof(GameSceneController);
            const string MethodName = nameof(GameSceneController.OnClickNextDay);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnClickNextDay_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = GameSceneControllerPatchOnClickNextDay.Postfix;

            action.ShouldThrow<NullReferenceException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<NullReferenceException>()), Times.Once);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnStageStart_is_patched_correctly()
        {
            var patch = typeof(GameSceneControllerPatchOnStageStart);
            var originalClass = typeof(GameSceneController);
            const string MethodName = nameof(GameSceneController.OnStageStart);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_GameSceneController_Method_OnStageStart_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = GameSceneControllerPatchOnStageStart.Postfix;

            action.ShouldThrow<NullReferenceException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<NullReferenceException>()), Times.Once);
        }

        [Fact]
        public void Class_NewTitleScript_Method_OnClickNewGame_is_patched_correctly()
        {
            var patch = typeof(NewTitleScriptPatchOnClickNewGame);
            var originalClass = typeof(NewTitleScript);
            const string MethodName = nameof(NewTitleScript.OnClickNewGame);

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_NewTitleScript_Method_OnClickNewGame_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = NewTitleScriptPatchOnClickNewGame.Postfix;

            action.ShouldThrow<NullReferenceException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<NullReferenceException>()), Times.Once);
        }

        [Fact]
        public void Class_UseSkill_Method_FinishWorkSuccessfully_is_patched_correctly()
        {
            var patch = typeof(UseSkillPatchFinishWorkSuccessfully);
            var originalClass = typeof(UseSkill);
            const string MethodName = "FinishWorkSuccessfully";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_UseSkill_Method_FinishWorkSuccessfully_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);

            Action action = static () => UseSkillPatchFinishWorkSuccessfully.Prefix(null);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
        }
    }
}
