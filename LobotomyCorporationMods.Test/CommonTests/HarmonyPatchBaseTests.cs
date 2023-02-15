// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.CommonTests
{
    public sealed class HarmonyPatchBaseTests
    {
        private static readonly FakeHarmonyPatch s_fakeHarmonyPatch = new(true);

        [Fact]
        public void Instantiating_a_duplicate_static_instance_throws_an_exception()
        {
            Action action = static () =>
            {
                _ = new FakeHarmonyPatch(true);
            };

            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Initializing_patch_data_with_directory_does_not_error_and_initializes_logger()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Action action = () => s_fakeHarmonyPatch.TestInitializePatchData(new List<DirectoryInfo> { new(currentDirectory) });

            action.ShouldNotThrow();
            s_fakeHarmonyPatch.Logger.Should().NotBeNull();
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void Harmony_patch_exceptions_are_logged()
        {
            var mockLogger = new Mock<ILogger>();

            Action action = () => s_fakeHarmonyPatch.ApplyHarmonyPatch(null, null, mockLogger.Object);

            action.ShouldThrow<ArgumentNullException>();
            mockLogger.Verify(static logger => logger.WriteToLog(It.IsAny<ArgumentNullException>()), Times.Once);
        }
    }

    /// <summary>
    ///     Only to be used for HarmonyPatchBase tests.
    /// </summary>
    internal sealed class FakeHarmonyPatch : HarmonyPatchBase
    {
        internal FakeHarmonyPatch(bool isNotDuplicating) : base(isNotDuplicating)
        {
        }

        internal void TestInitializePatchData(ICollection<DirectoryInfo> directoryList)
        {
            InitializePatchData(typeof(FakeHarmonyPatch), "LobotomyCorporationMods.Test.dll", directoryList);
        }

        internal void ApplyHarmonyPatch([NotNull] Type harmonyPatchType, string modFileName, [NotNull] ILogger logger)
        {
            LoadData(logger);
            Instance.LoadData(logger);

            base.ApplyHarmonyPatch(harmonyPatchType, modFileName);
        }
    }
}
