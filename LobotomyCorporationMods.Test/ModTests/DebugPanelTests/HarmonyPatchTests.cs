// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel;
using LobotomyCorporationMods.DebugPanel.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class HarmonyPatchTests
    {
        [Fact]
        public void Constructor_is_public_and_externally_accessible()
        {
            Action action = () =>
            {
                _ = new Harmony_Patch();
            };

            _ = action.Should().NotThrow();
        }

        [Fact]
        public void Class_GlobalGameManager_Method_Awake_is_patched_correctly()
        {
            var patch = typeof(GlobalGameManagerPatchAwake);
            var originalClass = typeof(GlobalGameManager);
            const string MethodName = "Awake";

            patch.ValidateHarmonyPatch(originalClass, MethodName);
        }

        [Fact]
        public void Class_GlobalGameManager_Method_Awake_logs_exceptions()
        {
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

            static void Action()
            {
                GlobalGameManagerPatchAwake.PostfixWithLogging(null!, null!);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }
    }
}
