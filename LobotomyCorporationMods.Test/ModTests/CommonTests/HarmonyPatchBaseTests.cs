// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AwesomeAssertions;
using JetBrains.Annotations;
using Hemocode.Common.Extensions;
using Hemocode.Common.Implementations;
using Hemocode.Common.Interfaces;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests
{
    public sealed class HarmonyPatchBaseTests
    {
        private FakeHarmonyPatch _fakeHarmonyPatch = new(false);

        [Fact]
        public void Applying_a_Harmony_patch_does_not_error()
        {
            Mock<ILogger> mockLogger = new();

            Action action = () => _fakeHarmonyPatch.ApplyHarmonyPatch(typeof(HarmonyPatchBase), string.Empty, mockLogger.Object);

            _ = action.Should().NotThrow();
        }

        [Fact]
        public void Harmony_patch_exceptions_are_logged()
        {
            Mock<ILogger> mockLogger = new();

            void Action()
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                // Forcing null argument to test exception logging.
                _fakeHarmonyPatch.ApplyHarmonyPatch(null, string.Empty, mockLogger.Object);
            }

            mockLogger.VerifyArgumentNullException(Action);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Test Text")]
        public void Initializing_localized_text_returns_correct_value(string expectedValue)
        {
            // Arrange
            var key = nameof(Initializing_localized_text_returns_correct_value) + expectedValue;
            Dictionary<string, string> dictionary = new()
            {
                {
                    key, expectedValue
                },
            };

            LocalizeTextDataModel.instance.Init(dictionary);

            // Act
            var dictionaryValue = key.GetLocalized();

            // Assert
            _ = dictionaryValue.Should().Be(expectedValue).And.NotBe(LocalizeTextDataModel.Failed);
        }

        [Fact]
        public void Initializing_patch_data_with_directory_does_not_error_and_initializes_logger()
        {
            InitiatePatchData();
            AssertInitialization(false);
        }

        [Fact]
        public void Initializing_patch_data_with_english_localization_file_does_not_error_and_initializes_logger()
        {
            GenerateXmlLocalizationFile();
            InitiatePatchData();
            AssertInitialization(false);

            // Cleanup
            DeleteXmlLocalizationFile();
        }

        [Fact]
        public void Trying_to_initialize_patch_without_inheriting_from_base_does_not_initialize()
        {
            InitiatePatchData(typeof(object));
            AssertInitialization(true);
        }

        [Fact]
        public void Instantiating_a_duplicate_static_instance_throws_an_exception()
        {
            _fakeHarmonyPatch = new FakeHarmonyPatch(true);

            Action action = () =>
            {
                _ = new FakeHarmonyPatch(true);
            };

            _ = action.Should().Throw<InvalidOperationException>();
        }

        #region Helper Methods

        private void AssertInitialization(bool isNull)
        {
            _ = isNull ? _fakeHarmonyPatch.Logger.Should().BeNull() : _fakeHarmonyPatch.Logger.Should().NotBeNull();
        }

        private static void GenerateXmlLocalizationFile()
        {
            const string XmlTextId = "testId";
            const string XmlTextValue = "Test Text";

            StringBuilder xmlTextBuilder = new();
            _ = xmlTextBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            _ = xmlTextBuilder.AppendLine("<localize>");
            _ = xmlTextBuilder.AppendLine($"    <text id=\"{XmlTextId}\">{XmlTextValue}</text>\n");
            _ = xmlTextBuilder.AppendLine("</localize>");

            const string LocalizationFile = "Localize/en/text_en.xml";
            var fileManager = TestExtensions.GetMockFileManager();
            var fileWithPath = fileManager.Object.GetFile(LocalizationFile);
            fileManager.Object.WriteAllText(fileWithPath, xmlTextBuilder.ToString());
        }

        private static void DeleteXmlLocalizationFile()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            const string LocalizationFile = "Localize/en/text_en.xml";
            File.Delete(Path.Combine(currentDirectory, LocalizationFile));
        }

        private void InitiatePatchData([CanBeNull] Type? patchType = null)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Action action = () => _fakeHarmonyPatch.TestInitializePatchData(
            [
                new DirectoryInfo(currentDirectory),
            ], patchType);

            _ = action.Should().NotThrow();
        }

        #endregion
    }

    /// <summary>Only to be used for HarmonyPatchBase tests.</summary>
    public sealed class FakeHarmonyPatch : HarmonyPatchBase
    {
        private const string FileNameThatExists = "FileNameThatExists.txt";

        internal FakeHarmonyPatch(bool isNotDuplicating) : base(typeof(FakeHarmonyPatch), nameof(FakeHarmonyPatch), isNotDuplicating)
        {
        }

        internal void ApplyHarmonyPatch([NotNull] Type? harmonyPatchType,
            string modFileName,
            ILogger logger)
        {
            AddLoggerTarget(logger);
            Instance.AddLoggerTarget(logger);

            ApplyHarmonyPatch(harmonyPatchType, modFileName);
        }

        internal void TestInitializePatchData([NotNull] ICollection<DirectoryInfo> directoryList,
            Type? patchType = null)
        {
            patchType = patchType.EnsureNotNullWithMethod(() => typeof(FakeHarmonyPatch));

            var directory = directoryList.First();
            var testFileWithPath = Path.Combine(directory.FullName, FileNameThatExists);
            File.WriteAllText(testFileWithPath, string.Empty);

            SetUpPatchData(patchType, FileNameThatExists, directoryList);
        }
    }
}
