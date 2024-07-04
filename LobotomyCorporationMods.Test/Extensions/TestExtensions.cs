// SPDX-License-Identifier: MIT

#region

using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Interfaces;
using Moq;
using ILogger = LobotomyCorporationMods.Common.Interfaces.ILogger;

#endregion

// ReSharper disable MemberCanBePrivate.Global
namespace LobotomyCorporationMods.Test.Extensions
{
    internal static class TestExtensions
    {
        [NotNull]
        internal static Mock<IFileManager> GetMockFileManager()
        {
            var mockFileManager = new Mock<IFileManager>();
            _ = mockFileManager.Setup(fm => fm.GetOrCreateFile(It.IsAny<string>())).Returns((string fileName) => fileName.InCurrentDirectory());
            _ = mockFileManager.Setup(fm => fm.ReadAllText(It.IsAny<string>(), It.IsAny<bool>())).Returns((string fileName,
                bool _) => File.ReadAllText(fileName.InCurrentDirectory()));

            return mockFileManager;
        }

        [NotNull]
        internal static Mock<ILogger> GetMockLogger()
        {
            var mockLogger = new Mock<ILogger>();

            return mockLogger;
        }

        [NotNull]
        internal static string InCurrentDirectory([NotNull] this string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        internal static void ValidateHarmonyPatch([NotNull] this MemberInfo patchClass,
            Type originalClass,
            string methodName)
        {
            var attribute = Attribute.GetCustomAttribute(patchClass, typeof(HarmonyPatch)) as HarmonyPatch;

            attribute.Should().NotBeNull();
            attribute?.info.originalType.Should().Be(originalClass);
            attribute?.info.methodName.Should().Be(methodName);
        }

        internal static void VerifyArgumentNullException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<ArgumentNullException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<ArgumentNullException>()), numberOfTimes ?? Times.Once());
        }

        internal static void VerifyNullReferenceException([NotNull] this Mock<ILogger> mockLogger,
            Action action,
            Times? numberOfTimes = null)
        {
            action.Should().Throw<NullReferenceException>();
            mockLogger.Verify(logger => logger.WriteException(It.IsAny<NullReferenceException>()), numberOfTimes ?? Times.Once());
        }
    }
}
