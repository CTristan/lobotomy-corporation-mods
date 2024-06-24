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
        private static readonly FakeHarmonyPatch s_fakeHarmonyPatch = new FakeHarmonyPatch(true);

        [Fact]
        public void Applying_a_Harmony_patch_does_not_error()
        {
            var mockLogger = new Mock<ILogger>();

            Action action = () =>
                s_fakeHarmonyPatch.ApplyHarmonyPatch(typeof(HarmonyPatchBase), string.Empty, mockLogger.Object);

            action.Should().NotThrow();
        }

        [Fact]
        public void Harmony_patch_exceptions_are_logged()
        {
            var mockLogger = new Mock<ILogger>();

            void Action()
            {
                s_fakeHarmonyPatch.ApplyHarmonyPatch(null, string.Empty, mockLogger.Object);
            }

            mockLogger.VerifyExceptionLogged<ArgumentNullException>(Action);
        }

        [Fact]
        public void Initializing_patch_data_with_directory_does_not_error_and_initializes_logger()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Action action = () =>
                s_fakeHarmonyPatch.TestInitializePatchData(new List<DirectoryInfo>
                {
                    new DirectoryInfo(currentDirectory)
                });

            action.Should().NotThrow();
            s_fakeHarmonyPatch.Logger.Should().NotBeNull();
        }

        [Fact]
        public void Instantiating_a_duplicate_static_instance_throws_an_exception()
        {
            Action action = () =>
            {
                _ = new FakeHarmonyPatch(true);
            };

            action.Should().Throw<InvalidOperationException>();
        }
    }

    /// <summary>
    ///     Only to be used for HarmonyPatchBase tests.
    /// </summary>
    internal sealed class FakeHarmonyPatch : HarmonyPatchBase
    {
        private const string FileNameThatExists = "FileNameThatExists.txt";

        internal FakeHarmonyPatch(bool isNotDuplicating)
            : base(isNotDuplicating)
        {
        }

        internal void ApplyHarmonyPatch(Type harmonyPatchType, string modFileName, ILogger logger)
        {
            LoadData(logger);
            Instance.LoadData(logger);

            ApplyHarmonyPatch(harmonyPatchType, modFileName);
        }

        internal void TestInitializePatchData([NotNull] ICollection<DirectoryInfo> directoryList)
        {
            var directory = directoryList.First();
            var testFileWithPath = Path.Combine(directory.FullName, FileNameThatExists);
            File.WriteAllText(testFileWithPath, string.Empty);

            InitializePatchData(typeof(FakeHarmonyPatch), FileNameThatExists, directoryList);
        }
    }
}
