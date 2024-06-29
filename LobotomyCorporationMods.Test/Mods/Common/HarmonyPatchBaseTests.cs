// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.Mods.Common
{
    public sealed class HarmonyPatchBaseTests
    {
        private FakeHarmonyPatch _fakeHarmonyPatch = new FakeHarmonyPatch(false);

        [Fact]
        public void Applying_a_Harmony_patch_does_not_error()
        {
            var mockLogger = new Mock<ILogger>();

            Action action = () => _fakeHarmonyPatch.ApplyHarmonyPatch(typeof(HarmonyPatchBase), string.Empty, mockLogger.Object);

            action.Should().NotThrow();
        }

        [Fact]
        public void Harmony_patch_exceptions_are_logged()
        {
            var mockLogger = new Mock<ILogger>();

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                _fakeHarmonyPatch.ApplyHarmonyPatch(null, string.Empty, mockLogger.Object);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Fact]
        public void Initializing_patch_data_with_directory_does_not_error_and_initializes_logger()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Action action = () => _fakeHarmonyPatch.TestInitializePatchData(new List<DirectoryInfo>
            {
                new DirectoryInfo(currentDirectory),
            });

            action.Should().NotThrow();
            _fakeHarmonyPatch.Logger.Should().NotBeNull();
        }

        [Fact]
        public void Trying_to_initialize_patch_without_inheriting_from_base_does_not_initialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Action action = () => _fakeHarmonyPatch.TestInitializePatchData(new List<DirectoryInfo>
            {
                new DirectoryInfo(currentDirectory),
            }, typeof(object));

            action.Should().NotThrow();
            _fakeHarmonyPatch.Logger.Should().BeNull();
        }


        [Fact]
        public void Instantiating_a_duplicate_static_instance_throws_an_exception()
        {
            _fakeHarmonyPatch = new FakeHarmonyPatch(true);

            Action action = () =>
            {
                _ = new FakeHarmonyPatch(true);
            };

            action.Should().Throw<InvalidOperationException>();
        }
    }

    /// <summary>Only to be used for HarmonyPatchBase tests.</summary>
    internal sealed class FakeHarmonyPatch : HarmonyPatchBase
    {
        private const string FileNameThatExists = "FileNameThatExists.txt";

        internal FakeHarmonyPatch(bool isNotDuplicating) : base(typeof(FakeHarmonyPatch), nameof(FakeHarmonyPatch), isNotDuplicating)
        {
        }

        internal void ApplyHarmonyPatch([NotNull] Type harmonyPatchType,
            string modFileName,
            ILogger logger)
        {
            AddLoggerTarget(logger);
            Instance.AddLoggerTarget(logger);

            ApplyHarmonyPatch(harmonyPatchType, modFileName);
        }

        internal void TestInitializePatchData([NotNull] ICollection<DirectoryInfo> directoryList,
            Type patchType = null)
        {
            patchType = TestExtensions.EnsureNotNullWithDefault(patchType, () => typeof(FakeHarmonyPatch));

            var directory = directoryList.First();
            var testFileWithPath = Path.Combine(directory.FullName, FileNameThatExists);
            File.WriteAllText(testFileWithPath, string.Empty);

            SetUpPatchData(patchType, FileNameThatExists, directoryList);
        }
    }
}
